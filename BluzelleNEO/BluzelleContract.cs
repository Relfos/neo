using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Bluzelle.NEO.Contract
{
    public class BluzelleContract : SmartContract
    {
        public static readonly byte[] Admin_Address = "AHKPx5dZYnwAweQUJQH3UefoswKm6beEz2".ToScriptHash();

        private static readonly byte[] request_owner_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'O' };
        private static readonly byte[] request_count_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'C' };
        private static readonly byte[] request_key_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'K' };
        private static readonly byte[] request_value_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'V' };
        private static readonly byte[] request_result_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'R' };

        private static readonly byte[] request_create = { (byte)'C' };
        private static readonly byte[] request_read = { (byte)'R' };
        private static readonly byte[] request_update = { (byte)'U' };
        private static readonly byte[] request_delete = { (byte)'D' };

        private static readonly byte[] value_null = {0};

        public static object Main(string operation, object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                if (Runtime.CheckWitness(Admin_Address))
                {
                    return true;
                }

                return false;
            }
            else
            {
                if (operation == "create")
                {
                    if (args.Length != 3) return false;
                    byte[] address = (byte[])args[0];
                    byte[] key = (byte[])args[0];
                    byte[] value = (byte[])args[0];
                    return API_Create(address, key, value);
                }

                if (operation == "read")
                {
                    if (args.Length != 2) return false;
                    byte[] address = (byte[])args[0];
                    byte[] key = (byte[])args[0];
                    return API_Read(address, key);
                }

                if (operation == "update")
                {
                    if (args.Length != 3) return false;
                    byte[] address = (byte[])args[0];
                    byte[] key = (byte[])args[0];
                    byte[] value = (byte[])args[0];
                    return API_Update(address, key, value);
                }

                if (operation == "delete")
                {
                    if (args.Length != 2) return false;
                    byte[] address = (byte[])args[0];
                    byte[] key = (byte[])args[0];
                    return API_Delete(address, key);
                }

                if (operation == "push")
                {
                    if (args.Length != 2) return false;
                    BigInteger id = (BigInteger)args[0];
                    byte[] value = (byte[])args[0];
                    return API_Push(id, value);
                }

                if (operation == "check")
                {
                    if (args.Length != 1) return false;
                    BigInteger id = (BigInteger)args[0];
                    return API_Pull(id);
                }

                return false;
            }
        }

        private static BigInteger API_Create(byte[] scripthash, byte[] key, byte[] value)
        {
            return MakeRequest(scripthash, request_create, key, value);
        }

        private static BigInteger API_Read(byte[] scripthash, byte[] key)
        {
            return MakeRequest(scripthash, request_read, key, value_null);
        }

        private static BigInteger API_Update(byte[] scripthash, byte[] key, byte[] value)
        {
            return MakeRequest(scripthash, request_update, key, value);
        }

        private static BigInteger API_Delete(byte[] scripthash, byte[] key)
        {
            return MakeRequest(scripthash, request_delete, key, value_null);
        }

        private static bool API_Push(BigInteger req_id, byte[] value)
        {
            if (!Runtime.CheckWitness(Admin_Address))
            {
                return false;
            }

            var req_result_key = request_key_prefix.Concat(req_id.AsByteArray());
            Storage.Put(Storage.CurrentContext, req_result_key, value);

            return true;
        }

        private static byte[] API_Pull(BigInteger req_id)
        {
            var req_result_key = request_key_prefix.Concat(req_id.AsByteArray());
            return Storage.Get(Storage.CurrentContext, req_result_key);
        }

        private static BigInteger MakeRequest(byte[] scriptHash, byte[] operation, byte[] key, byte[] value)
        {
            if (!Runtime.CheckWitness(scriptHash))
            {
                return 0;
            }

            var req_count = Storage.Get(Storage.CurrentContext, request_count_prefix).AsBigInteger();
            req_count = req_count + 1;

            var req_owner_key = request_owner_prefix.Concat(req_count.AsByteArray());
            Storage.Put(Storage.CurrentContext, req_owner_key, scriptHash);

            var req_key_key = request_key_prefix.Concat(req_count.AsByteArray());
            Storage.Put(Storage.CurrentContext, req_key_key, operation.Concat(key));

            if (value.AsBigInteger() != 0)
            {
                var req_val_key = request_value_prefix.Concat(req_count.AsByteArray());
                Storage.Put(Storage.CurrentContext, req_val_key, value);
            }

            return req_count;
        }
    }
}
