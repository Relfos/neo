using LunarParser;
using LunarParser.JSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Bluzelle.NEO.Sharp.Core
{
    public struct WSRequest
    {
        public enum Command
        {
            Create,
            Read,
            Update,
            Remove
        }

        public Command command;
        public Dictionary<string, string> data;
        public string uuid;
    }

    public class WSSwarm : ISwarm
    {
        private string url;
        private int request_id;

        public WSSwarm(string url)
        {
            this.url = url;
        }

        private string BuildRequest(WSRequest request)
        {
            var root = DataNode.CreateObject();
            root.AddField("bzn-api", "crud");
            root.AddField("cmd", request.command.ToString().ToLower());

            var node = DataNode.CreateObject("data");
            foreach (var entry in request.data)
            {
                node.AddField(entry.Key, entry.Value);
            }
            root.AddNode(node);

            root.AddField("db-uuid", request.uuid);
            root.AddField("request-id", request_id);

            var json = JSONWriter.WriteToString(root);

            return json;
        }

        private async Task<string> SendRequestToSocket(string json)
        {
            string response = null;
            bool failed = false;

            using (var ws = new WebSocket(url))
            {
                ws.OnError += (sender, e) =>
                {
                    failed = true;
                    //Console.WriteLine("error: " + e.ToString());
                };

                ws.OnMessage += (sender, e) =>
                {
                    response = e.Data;
                };

                /*ws.OnClose += (sender, e) =>
                {
                    Console.WriteLine("closed socket");
                };*/


                System.IO.File.WriteAllText("dump.json", json);
                ws.Connect();

                try
                {
                    ws.Send(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }

                while (response == null)
                {
                    await Task.Delay(100);
                    if (failed)
                    {
                        return null;
                    }
                }

                return response;
            }
        }

        private async Task<string> DoRequest(WSRequest request)
        {
            request_id++;

            var json = BuildRequest(request);

            var response = await SendRequestToSocket(json);

            if (response == null)
            {
                return null;
            }

            var root = JSONReader.ReadFromString(response);
            if (root.HasNode("error"))
            {
                var error = root.GetString("error");
                if (error == "NOT_THE_LEADER")
                {
                    var data = root["data"];
                    var leaderHost = data.GetString("leader-host");
                    var leaderPort = data.GetInt32("leader-port");
                    this.url = $"ws://{leaderHost}:{leaderPort}";

                    return await SendRequestToSocket(json);
                }
            }

            return response;
        }

        public async Task<bool> Create(string uuid, string key, string value)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;
            //args["value"] = Convert.ToBase64String(value);
            args["value"] = value;

            var request = new WSRequest() { command = WSRequest.Command.Create, data = args, uuid = uuid };
            var response = await DoRequest(request);
            return response != null && !response.Contains("error");
        }

        public async Task<string> Read(string uuid, string key)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;

            var request = new WSRequest() { command = WSRequest.Command.Read, data = args, uuid = uuid };
            var json = await DoRequest(request);            
            if (json == null)
            {
                return null;
            }

            var root = JSONReader.ReadFromString(json);
            if (root.HasNode("data"))
            {
                var data = root["data"];
                return data.GetString("value");
            }

            return null;
        }

        public async Task<bool> Remove(string uuid, string key)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;

            var request = new WSRequest() { command = WSRequest.Command.Remove, data = args, uuid = uuid };
            var response = await DoRequest(request);
            return response != null && !response.Contains("error");
        }

        public async Task<bool> Update(string uuid, string key, string value)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;
            args["value"] = value;
            //args["value"] = Convert.ToBase64String(value);

            var request = new WSRequest() { command = WSRequest.Command.Update, data = args, uuid = uuid };
            var response = await DoRequest(request);
            return response != null && !response.Contains("error");
        }
    }
}
