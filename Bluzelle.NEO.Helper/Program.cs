using Bluzelle.NEO.Sharp.Core;
using Neo.Lux.Core;
using Neo.Lux.Utils;
using Neo.Lux.Cryptography;
using System;
using System.IO;

namespace Bluzelle.NEO.Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings(args);

            Console.WriteLine("Loading Bluzelle contract bytecode...");

            var contractFile = settings.GetValue("avm.path");
            if (!File.Exists(contractFile))
            {
                Console.WriteLine($"The file '{contractFile}' was not found");
                Environment.Exit(-1);
            }

            var contractBytes = File.ReadAllBytes(contractFile);
            var contractHash = contractBytes.ToScriptHash();

            var api = new CustomRPCNode(settings.GetValue("rpc.host", "localhost"), int.Parse(settings.GetValue("rpc.port", "30333")),
                settings.GetValue("neoscan.host", "localhost"), int.Parse(settings.GetValue("neoscan.port", "4000")));


            Console.WriteLine("******Bluzelle Tool******");

            var uuid = "92b8bac6-3242-452a-9090-1aa48afd71a3";

            //var swarm = new WSSwarm("ws://192.168.138.134:51010");
            //var swarm = new WSSwarm("ws://13.78.131.94:51010");
            var swarm = new WSSwarm("ws://testnet.bluzelle.com:51010");

            //var owner_keys = KeyPair.FromWIF("L3Vo5HcJhDoL7s81i4PSDTPfbUpVPrFHQ3V1GwSESkQtF4LW2vvJ");
            var owner_keys = KeyPair.FromWIF("KxDgvEKzgSBPPfuVfw67oPQBSjidEiqTHURKSDL1R7yGaGYAeYnr");

            do
            {
                Console.WriteLine("0 - Exit");
                Console.WriteLine("1 - Swarm.Create");
                Console.WriteLine("2 - Swarm.Read");
                Console.WriteLine("3 - Swarm.Update");
                Console.WriteLine("4 - Swarm.Delete");
                Console.WriteLine("5 - Contract.Write");
                Console.WriteLine("6 - Contract.Read");
                Console.WriteLine("7 - Contract.Deploy");

                int option;
                
                if (int.TryParse(Console.ReadLine(), out option))
                {
                    switch (option){
                        case 0:
                            {
                                Environment.Exit(0);
                                break;
                            }

                        case 1:
                            {
                                Console.WriteLine("**CREATE**");

                                Console.Write("KEY: ");
                                var key = Console.ReadLine();

                                Console.Write("VALUE: ");
                                var val = Console.ReadLine();

                                var result = swarm.Create(uuid, key, val).GetAwaiter().GetResult();
                                if (result)
                                {
                                    Console.WriteLine("Updated!");
                                }
                                else
                                {
                                    Console.WriteLine("Failed!");
                                }

                                break;
                            }

                        case 2:
                            {
                                Console.WriteLine("**READ**");

                                Console.Write("KEY: ");
                                var key = Console.ReadLine();

                                var val = swarm.Read(uuid, key).GetAwaiter().GetResult();
                                if  (val != null)
                                {
                                    Console.WriteLine("GOT: " + val);
                                }
                                else
                                {
                                    Console.WriteLine("Nothing found");
                                }

                                break;
                            }

                        case 3:
                            {
                                Console.WriteLine("**UPDATE**");

                                Console.Write("KEY: ");
                                var key = Console.ReadLine();

                                Console.Write("VALUE: ");
                                var val = Console.ReadLine();

                                var result = swarm.Update(uuid, key, val).GetAwaiter().GetResult();
                                if (result)
                                {
                                    Console.WriteLine("Updated!");
                                }
                                else
                                {
                                    Console.WriteLine("Failed!");
                                }

                                break;
                            }

                        case 4:
                            {
                                Console.WriteLine("**DELETE**");

                                Console.Write("KEY: ");
                                var key = Console.ReadLine();

                                var result = swarm.Delete(uuid, key).GetAwaiter().GetResult();
                                if (result)
                                {
                                    Console.WriteLine("Removed!");
                                }
                                else
                                {
                                    Console.WriteLine("Failed!");
                                }

                                break;
                            }

                        case 5:
                            {
                                Console.WriteLine("**CONTRACT.WRITE**");

                                Console.Write("KEY: ");
                                var key = Console.ReadLine();

                                Console.Write("VALUE: ");
                                var content = Console.ReadLine();

                                var tx = api.CallContract(owner_keys, contractHash, "update", new object[] { owner_keys.address.AddressToScriptHash(), uuid, key, content });

                                if (tx != null)
                                {
                                    Console.WriteLine($"Unconfirmed tx: {tx.Hash}");
                                    api.WaitForTransaction(owner_keys, tx);
                                    Console.WriteLine($"Confirmed tx: {tx.Hash}");
                                }
                                else
                                {
                                    Console.WriteLine("Tx Failed!");
                                }

                                break;
                            }

                        case 6:
                            {
                                Console.WriteLine("**CONTRACT.READ**");


                                Console.Write("KEY: ");
                                var key = Console.ReadLine();

                                var tx = api.CallContract(owner_keys, contractHash, "read", new object[] { owner_keys.address.AddressToScriptHash(), uuid, key });

                                if (tx != null)
                                {
                                    Console.WriteLine($"Unconfirmed tx: {tx.Hash}");
                                    api.WaitForTransaction(owner_keys, tx);
                                    Console.WriteLine($"Confirmed tx: {tx.Hash}");

                                    var val = swarm.Read(uuid, key).GetAwaiter().GetResult();
                                    if (val != null)
                                    {
                                        Console.WriteLine("GOT: " + val);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Nothing found");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Tx Failed!");
                                }

                                break;
                            }


                        case 7:
                            {
                                Console.WriteLine("**CONTRACT.DEPLOY**");

                                var tx = api.DeployContract(owner_keys, contractBytes, new byte[] { 0x07, 0x10 }, 0x05, ContractPropertyState.HasStorage, "Bluzelle", "v1.0", "Bluzelle", "info@bluzelle.io", "contract");
                                if (tx != null)
                                {
                                    Console.WriteLine($"Unconfirmed tx: {tx.Hash}");
                                    api.WaitForTransaction(owner_keys, tx);
                                    Console.WriteLine($"Confirmed tx: {tx.Hash}");
                                }
                                else
                                {
                                    Console.WriteLine("Tx Failed!");
                                }

                                break;
                            }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid option");
                }

            } while (true);

        }
    }
}
