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

        private async Task<string> DoRequest(WSRequest request)
        {
            request_id++;

            string response = null;
            bool error = false;

            using (var ws = new WebSocket(url))
            {
                ws.OnError += (sender, e) =>
                {
                    error = true;
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
                    if (error)
                    {
                        return null;
                    }
                }

                return response;
            }
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

            var request = new WSRequest() { command = WSRequest.Command.Create, data = args, uuid = uuid };
            var response = await DoRequest(request);
            return response != null && !response.Contains("error");
        }
    }
}
