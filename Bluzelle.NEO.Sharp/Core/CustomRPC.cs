using Neo.Lux.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bluzelle.NEO.Sharp.Core
{
    public class CustomRPCNode : NeoRPC
    {
        private string host;

        public CustomRPCNode(string host, int rpc_port, int neoscan_port) : base(rpc_port, $"http://{host}:{neoscan_port}")
        {
            this.host = host;
        }

        protected override string GetRPCEndpoint()
        {
            return $"http://{host}:{port}";
        }
    }
}
