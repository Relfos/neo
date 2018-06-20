using LunarParser;
using LunarParser.JSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;
using Google.Protobuf;
using System.IO;
using System.Text;

namespace Bluzelle.NEO.Sharp.Core
{
    public class WSSwarm : ISwarm
    {
        private string url;
        private ulong request_id;

        public WSSwarm(string url)
        {
            this.url = url;
        }

        private string BuildRequest(bzn_msg msg, ulong txid,  string uuid)
        {
            var root = DataNode.CreateObject();
            msg.Db.Header = new database_header() { DbUuid = uuid, TransactionId = txid };
            root.AddField("bzn-api", "database");
            using (var output = new MemoryStream()) {
                msg.WriteTo(output);

                var data = Convert.ToBase64String(msg.ToByteArray());
                root.AddField("msg", data);
            }

            var json = JSONWriter.WriteToString(root);

            return json;
        }

        private async Task<database_response.Types.response> SendRequestToSocket(string json)
        {
            byte[] response = null;
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
                    response = e.RawData;
                };

                ws.OnClose += (sender, e) =>
                {
                  //  Console.WriteLine("closed socket");
                };


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

                using (var stream = new MemoryStream(response))
                {
                    var obj = database_response.Parser.ParseFrom(stream);
                    if (obj.Redirect != null) {
                        this.url = $"ws://{obj.Redirect.LeaderHost}:{obj.Redirect.LeaderPort}";

                        return await SendRequestToSocket(json);
                    }

                    return obj.Resp;
                }

            }
        }

        private async Task<database_response.Types.response> DoRequest(bzn_msg msg, string uuid)
        {
            request_id++;

            var json = BuildRequest(msg, request_id, uuid);

            var response = await SendRequestToSocket(json);

            return response;
        }

        public async Task<bool> Create(string uuid, string key, string value)
        {
            var msg = new bzn_msg() { Db = new database_msg() { Create = new database_create() { Key = key, Value = ByteString.CopyFrom(value, Encoding.UTF8) } } };
            var response = await DoRequest(msg, uuid);
            return response != null && !string.IsNullOrEmpty(response.Error);
        }

        public async Task<string> Read(string uuid, string key)
        {
            var msg = new bzn_msg() { Db = new database_msg() { Read = new database_read() { Key = key } } };
            var response = await DoRequest(msg, uuid);

            if (response == null || !string.IsNullOrEmpty(response.Error)) {
                return null;
            }

            return Encoding.UTF8.GetString(response.Value.ToByteArray());
        }

        public async Task<bool> Delete(string uuid, string key)
        {
            var msg = new bzn_msg() { Db = new database_msg() { Delete = new database_delete() { Key = key } } };
            var response = await DoRequest(msg, uuid);
            return response != null && !string.IsNullOrEmpty(response.Error);
        }

        public async Task<bool> Update(string uuid, string key, string value)
        {
            var msg = new bzn_msg() { Db = new database_msg() { Update = new database_update() { Key = key, Value = ByteString.CopyFrom(value, Encoding.UTF8) } } };
            var response = await DoRequest(msg, uuid);
            var result = response == null || !string.IsNullOrEmpty(response.Error);
            return result; 
        }
    }
}
