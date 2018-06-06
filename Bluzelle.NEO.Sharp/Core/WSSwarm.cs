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

        }

        private async Task<bool> DoRequest(WSRequest request)
        {
            request_id++;

            string response = null;

            using (var ws = new WebSocket(url))
            {
                ws.OnMessage += (sender, e) =>
                    {
                        response = e.Data;
                    };

                var root = DataNode.CreateObject();
                root.AddField("bzn-api", "crud");
                root.AddField("cmd", request.command.ToString().ToLower());

                var node = DataNode.CreateArray("data");
                foreach (var entry in request.data)
                {
                    node.AddField(entry.Key, entry.Value);
                }
                root.AddNode(node);

                root.AddField("db-uuid", request.uuid);
                root.AddField("request-id", request_id);

                var json = JSONWriter.WriteToString(root);

                ws.Connect();
                ws.Send(json);                

                while (response == null)
                {
                    await Task.Delay(100);
                }

                // TODO better error handling
                return !response.Contains("error");
            }
        }

        public async Task<bool> Create(string uuid, string key, byte[] value)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;
            args["value"] = Convert.ToBase64String(value);

            var request = new WSRequest() { command = WSRequest.Command.Create, data = args, uuid = uuid };
            return await DoRequest(request);
        }

        public async Task<byte[]> Read(string uuid, string key)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;

            var request = new WSRequest() { command = WSRequest.Command.Read, data = args, uuid = uuid };
            //return await DoRequest(request);
            return null;
        }

        public async Task<bool> Remove(string uuid, string key)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;

            var request = new WSRequest() { command = WSRequest.Command.Remove, data = args, uuid = uuid };
            return await DoRequest(request);
        }

        public async Task<bool> Update(string uuid, string key, byte[] value)
        {
            var args = new Dictionary<string, string>();
            args["key"] = key;
            args["value"] = Convert.ToBase64String(value);

            var request = new WSRequest() { command = WSRequest.Command.Create, data = args, uuid = uuid };
            return await DoRequest(request);
        }
    }
}
