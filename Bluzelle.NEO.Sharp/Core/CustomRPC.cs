using Neo.Lux.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bluzelle.NEO.Sharp.Core
{
    public class CustomRPCNode : NeoRPC
    {
        private string rpc_host;

        public CustomRPCNode(string rpc_host, int rpc_port, string neoscan_host, int neoscan_port) : base(rpc_port, $"http://{neoscan_host}:{neoscan_port}")
        {
            this.rpc_host = rpc_host;
        }

        protected override string GetRPCEndpoint()
        {
            return $"http://{rpc_host}:{port}";
        }
    }
}
