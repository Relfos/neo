using Neo.Lux.Core;
using System;
using Bluzelle.NEO.Tests;
using Bluzelle.NEO.Sharp.Core;

namespace Bluzelle.NEO.Bridge
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new LocalRPCNode(10332, "http://neoscan.io");
            //var api = new RemoteRPCNode(10332, "http://neoscan.io");
            //var api = new CustomRPCNode();

            Console.WriteLine("Running Bluzelle NEO bridge...");

            // test public address = AHKPx5dZYnwAweQUJQH3UefoswKm6beEz2
            var manager = new BridgeManager(api, new TestSwarm(), "L3Vo5HcJhDoL7s81i4PSDTPfbUpVPrFHQ3V1GwSESkQtF4LW2vvJ", @"..\..\bin\Debug\BluzelleContract.avm", api.GetBlockHeight() - 1);
            manager.Run();
        }
    }
}
