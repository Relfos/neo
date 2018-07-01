using Neo.Lux.Core;
using System;
using Bluzelle.NEO.Sharp.Core;
using System.IO;
using Neo.Lux.Utils;
using Neo.Lux.Cryptography;

namespace Bluzelle.NEO.Bridge
{
    public class CustomRPCNode : NeoRPC
    {
        private string host;

        public CustomRPCNode(string host, int rpc_port, int neoscan_port) : base(rpc_port, $"{host}:{neoscan_port}")
        {
            this.host = host;
        }

        protected override string GetRPCEndpoint()
        {
            return $"http://{host}:{port}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var host = args.Length > 0 ? args[0] : "localhost";
            Console.WriteLine("Connecting to neo rpc at " + host);
            var api = new CustomRPCNode(host, 30333, 4000);

            var lastBlock = api.GetBlockHeight() - 1;
            if (lastBlock < 0) lastBlock = 0;

     //       lastBlock = 20192;

            //var owner_keys = KeyPair.FromWIF("L3Vo5HcJhDoL7s81i4PSDTPfbUpVPrFHQ3V1GwSESkQtF4LW2vvJ");
            var owner_keys = KeyPair.FromWIF("KxDgvEKzgSBPPfuVfw67oPQBSjidEiqTHURKSDL1R7yGaGYAeYnr");            

            Console.WriteLine("Fetching balances");
            var balances = api.GetAssetBalancesOf("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y");
            if (balances == null || balances.Count == 0)
            {
                Console.WriteLine("Seems the private net is not currently running..");
                Environment.Exit(-1);
            }

            foreach (var entry in balances)
            {
                Console.WriteLine(entry.Key + " => " + entry.Value);
            }

            Console.WriteLine("Loading Bluzelle contract bytecode...");

            var contractFile = @"D:\code\crypto\BluzelleNeo\Bluzelle.NEO.Contract\bin\Debug\netcoreapp2.0\BluzelleContract.avm";
            if (!File.Exists(contractFile))
            {
                Console.WriteLine($"The file '{contractFile}' was not found");
                Environment.Exit(-1);
            }

            var contractBytes = File.ReadAllBytes(contractFile);
            var contractHash = contractBytes.ToScriptHash();

            Console.WriteLine($"Searching for contract at address {contractHash.ToAddress()}...");

            /*var test = api.InvokeScript(contractHash, new object[] { "test", new object[] { null} });
            var found = false;

            if (test != null && test.stack.Length > 0)
            {
                try
                {
                    var testResult = System.Text.Encoding.ASCII.GetString((byte[])test.stack[0]);
                    found = testResult.Equals("OK");
                }
                catch
                {
                    // skip
                }
            }

            if (found)
            {
                Console.WriteLine("Contract found in the NEO chain!");
            }
            else
            {
                Console.WriteLine("Contract not found in the NEO chain, deploying...");
                var tx = api.DeployContract(owner_keys, contractBytes, new byte[] { 0x07, 0x10 }, 0x05, ContractPropertyState.HasStorage, "Bluzelle", "1.0", "Bluzelle", "contact@bluzelle.io", "Bluzelle contract");
                api.WaitForTransaction(owner_keys, tx);

                Console.WriteLine("Contract is now deployed!");
            }
            */

            Console.WriteLine("Starting Bluzelle NEO bridge...");

//            var swarm = new WSSwarm("ws://192.168.138.134:51010");
            var swarm = new WSSwarm("ws://13.78.131.94:51010");

            // test public address = AHKPx5dZYnwAweQUJQH3UefoswKm6beEz2
            var manager = new BridgeManager(api, swarm, owner_keys, contractHash, lastBlock);
            manager.Run();
        }
    }
}
