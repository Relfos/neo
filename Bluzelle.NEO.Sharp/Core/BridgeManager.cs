﻿using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Debugger;
using Neo.Lux.Utils;
using Neo.Lux.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Bluzelle.NEO.Sharp.Core
{
    public class BridgeManager : IBlockchainProvider
    {
        private NeoAPI neo_api;
        private UInt160 bluzelle_contract_hash;
        private bool running;

        private SnapshotVM listenerVM;
        private ISwarm swarm;

        private KeyPair owner_keys;

        private Dictionary<UInt256, Transaction> transactions = new Dictionary<UInt256, Transaction>();

        private BigInteger lastRequestID = 0;

        public BridgeManager(NeoAPI api, ISwarm swarm, KeyPair owner_keys, UInt160 contract_hash, uint lastBlock)
        {
            this.neo_api = api;
            this.swarm = swarm;
            this.owner_keys = owner_keys;

            this.bluzelle_contract_hash = contract_hash;

            this.listenerVM = new SnapshotVM(this);

            // TODO: The last block should persistent between multiple sessions, in order to not miss any block
            this.lastBlock = lastBlock;
        }

        public void Stop()
        {
            if (running)
            {
                running = false;
            }
        }

        private uint lastBlock;

        public void Run(uint blockCount = 0)
        {
            if (running)
            {
                return;
            }

            this.running = true;

            bool forever = (blockCount == 0);
            
            do
            {
                var currentBlock = neo_api.GetBlockHeight();
                if (currentBlock > lastBlock)
                {
                    while (lastBlock < currentBlock)
                    {
                        ProcessIncomingBlock(lastBlock);
                        lastBlock++;

                        if (!forever)
                        {
                            blockCount--;
                            if (blockCount == 0)
                            {
                                return;
                            }
                        }
                    }
                }

                // sleeps 10 seconds in order to wait some time until next block is generated
                Thread.Sleep(10 * 1000);
            } while (running);
        }

        private void ProcessIncomingBlock(uint height)
        {
            var block = neo_api.GetBlock(height);

            if (block == null)
            {
                return;
            }

            Console.WriteLine($"Processing block {height}");

            foreach (var tx in block.transactions)
            {
                if (tx.type != TransactionType.InvocationTransaction)
                {
                    continue;
                }

                List<AVMInstruction> ops;

                try
                {
                    ops = NeoTools.Disassemble(tx.script);
                }
                catch
                {
                    continue;
                }

                for (int i = 0; i < ops.Count; i++)
                {
                    var op = ops[i];

                    // opcode data must contain the script hash to the Bluzelle contract, otherwise ignore it
                    if (op.opcode == OpCode.APPCALL && op.data != null && op.data.Length == 20)
                    {
                        var scriptHash = new UInt160(op.data);

                        if (scriptHash != bluzelle_contract_hash)
                        {
                            continue;
                        }

                        Console.WriteLine($"Found Bluzelle call in tx {tx.Hash}");

                        var operation = Encoding.ASCII.GetString(ops[i - 1].data);

                        int index = i - 3;
                        var argCount = 1 + ((byte)ops[index].opcode - (byte)OpCode.PUSH1);
                        var args = new List<object>();

                        while (argCount > 0)
                        {
                            index--;
                            if (ops[index].opcode >= OpCode.PUSHBYTES1 && ops[index].opcode <= OpCode.PUSHBYTES75)
                            {
                                args.Add(ops[index].data);
                            }
                            else
                            if (ops[index].opcode >= OpCode.PUSH1 && ops[index].opcode <= OpCode.PUSH16)
                            {
                                var n = new BigInteger(1 + (ops[index].opcode - OpCode.PUSH1));
                                args.Add(n);
                            }
                            else
                            if (ops[index].opcode == OpCode.PUSH0)
                            {
                                args.Add(new BigInteger(0));
                            }
                            else
                            if (ops[index].opcode == OpCode.PUSHM1)
                            {
                                args.Add(new BigInteger(-1));
                            }
                            else
                            {
                                throw new Exception("Invalid arg type");
                            }
                            argCount--;
                        }

                        switch (operation)
                        {
                            case "create":
                                {
                                    try
                                    {
                                        var uuid = Encoding.ASCII.GetString((byte[])args[1]);
                                        var key = Encoding.ASCII.GetString((byte[])args[2]);
                                        var value = Encoding.ASCII.GetString((byte[])args[3]);

                                        Console.WriteLine($"CREATE ({uuid},{key},{value})");
                                        this.swarm.Create(uuid, key, value);
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"Failed decoding args for tx {tx.Hash}");

                                    }


                                    break;
                                }

                            case "delete":
                                {
                                    try
                                    {
                                        var uuid = Encoding.ASCII.GetString((byte[])args[1]);
                                        var key = Encoding.ASCII.GetString((byte[])args[2]);

                                        Console.WriteLine($"DELETE ({uuid},{key})");
                                        this.swarm.Delete(uuid, key);
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"Failed decoding args for tx {tx.Hash}");

                                    }


                                    break;
                                }

                            case "update":
                                {
                                    try
                                    {
                                        var uuid = Encoding.ASCII.GetString((byte[])args[1]);
                                        var key = Encoding.ASCII.GetString((byte[])args[2]);
                                        var value = Encoding.ASCII.GetString((byte[])args[3]);

                                        Console.WriteLine($"UPDATE ({uuid},{key},{value})");
                                        this.swarm.Update(uuid, key, value);
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"Failed decoding args for tx {tx.Hash}");

                                    }


                                    break;
                                }

                            case "read":
                                {
                                    try
                                    {
                                        var uuid = Encoding.ASCII.GetString((byte[])args[1]);
                                        var key = Encoding.ASCII.GetString((byte[])args[2]);

                                        Console.WriteLine($"READ ({uuid},{key})");
                                        var val = this.swarm.Read(uuid, key);
                                        /*
                                        string id = null;
                                        var push_tx = neo_api.CallContract(owner_keys, bluzelle_contract_hash, "push", new object[] {id, val });
                                        neo_api.WaitForTransaction(owner_keys, push_tx);*/
                                    }
                                    catch
                                    {
//                                        Console.WriteLine($"Failed decoding args for tx {tx.Hash}");

                                    }


                                    break;
                                }

                        }

                        break;
                    }
                }

            }

        }

        /// <summary>
        /// Catches and processes all requests created by a Neo transaction
        /// </summary>
        /// <param name="tx"></param>
        private void ProcessRequests(Transaction tx)
        {
            // add the transaction to the cache
            transactions[tx.Hash] = tx;

            var storage = listenerVM.GetStorage(bluzelle_contract_hash);

            var bytes = storage.Get("REQC");
            var reqCount = new BigInteger(bytes);

            while (lastRequestID < reqCount)
            {
                lastRequestID++;
                ProcessRequest(storage, lastRequestID);
            }
        }

        private static readonly byte[] request_owner_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'O' };
        private static readonly byte[] request_key_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'K' };
        private static readonly byte[] request_value_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'V' };
        private static readonly byte[] request_uuid_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'U' };

        /// <summary>
        /// Fetches a request from the contract storage and processes it.
        /// </summary>
        private void ProcessRequest(Storage storage, BigInteger ID)
        {
            var base_key = ID.ToByteArray();

            var req_owner_key = request_owner_prefix.Concat(base_key).ToArray();
            var req_key_key = request_key_prefix.Concat(base_key).ToArray();
            var req_uuid_key = request_key_prefix.Concat(base_key).ToArray();
            var req_val_key = request_value_prefix.Concat(base_key).ToArray();

            var uuid = Encoding.UTF8.GetString(storage.Get(req_uuid_key));
            var value = Encoding.ASCII.GetString(storage.Get(req_val_key));
            var temp = storage.Get(req_key_key);
            var operation = (char) temp[0];
            var key = Encoding.UTF8.GetString(temp.Skip(1).ToArray());

            switch (operation)
            {
                case 'C':
                    {
                        this.swarm.Create(uuid, key, value);
                        break;
                    }

                case 'R':
                    {
                        var read = this.swarm.Read(uuid, key);

                        var push_tx = neo_api.CallContract(owner_keys, bluzelle_contract_hash, "api_push", new object[] { uuid, key, read });
                        neo_api.WaitForTransaction(owner_keys, push_tx);

                        break;
                    }

                case 'U':
                    {
                        this.swarm.Update(uuid, key, value);

                        break;
                    }

                case 'D':
                    {
                        this.swarm.Delete(uuid, key);
                        break;
                    }
            }
        }

        /// <summary>
        /// Fetches a transaction from local catch. If not found, will try fetching it from a NEO blockchain node
        /// </summary>
        /// <param name="hash">Hash of the transaction</param>
        /// <returns></returns>
        public Transaction GetTransaction(UInt256 hash)
        {
            return transactions.ContainsKey(hash) ? transactions[hash] : neo_api.GetTransaction(hash);
        }
    }
}
