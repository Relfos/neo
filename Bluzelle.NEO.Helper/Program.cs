using Bluzelle.NEO.Sharp.Core;
using System;

namespace Bluzelle.NEO.Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("******Bluzelle Helper******");

            var uuid = "92b8bac6-3242-452a-9090-1aa48afd71a3";

            //var swarm = new WSSwarm("ws://192.168.138.134:51010");
            var swarm = new WSSwarm("ws://13.78.131.94:51010");

            do
            {
                Console.WriteLine("0 - Exit");
                Console.WriteLine("1 - Swarm.Create");
                Console.WriteLine("2 - Swarm.Read");
                Console.WriteLine("3 - Swarm.Update");
                Console.WriteLine("4 - Swarm.Delete");

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
