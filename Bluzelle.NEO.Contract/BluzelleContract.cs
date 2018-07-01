using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace Bluzelle.NEO.Contract
{
    public class BluzelleContract : SmartContract
    {
        public static readonly byte[] Admin_Address = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();        

        private static readonly byte[] request_count_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'C' };
        private static readonly byte[] request_owner_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'O' };
        private static readonly byte[] request_key_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'K' };
        private static readonly byte[] request_value_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'V' };
        private static readonly byte[] request_uuid_prefix = { (byte)'R', (byte)'E', (byte)'Q', (byte)'U' };
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
                    if (args.Length != 4) return false;
                    byte[] address = (byte[])args[0];
                    byte[] uuid = (byte[])args[1];
                    byte[] key = (byte[])args[2];
                    byte[] value = (byte[])args[3];
                    return API_Create(address, uuid, key, value);
                }

                if (operation == "read")
                {
                    if (args.Length != 3) return false;
                    byte[] address = (byte[])args[0];
                    byte[] uuid = (byte[])args[1];
                    byte[] key = (byte[])args[2];
                    return API_Read(address, uuid, key);
                }

                if (operation == "update")
                {
                    if (args.Length != 4) return false;
                    byte[] address = (byte[])args[0];
                    byte[] uuid = (byte[])args[1];
                    byte[] key = (byte[])args[2];
                    byte[] value = (byte[])args[3];
                    return API_Update(address, uuid, key, value);
                }

                if (operation == "delete")
                {
                    if (args.Length != 3) return false;
                    byte[] address = (byte[])args[0];
                    byte[] uuid = (byte[])args[1];
                    byte[] key = (byte[])args[2];
                    return API_Delete(address, uuid, key);
                }

                if (operation == "push")
                {
                    if (args.Length != 2) return false;
                    BigInteger id = (BigInteger)args[0];
                    byte[] value = (byte[])args[1];
                    return API_Push(id, value);
                }

                if (operation == "pull")
                {
                    if (args.Length != 1) return false;
                    BigInteger id = (BigInteger)args[0];
                    return API_Pull(id);
                }

                if (operation == "test")
                {
                    return "OK";
                }

                return false;
            }
        }

        private static BigInteger API_Create(byte[] scripthash, byte[] uuid, byte[] key, byte[] value)
        {
            return MakeRequest(scripthash, request_create, uuid, key, value);
        }

        private static BigInteger API_Read(byte[] scripthash, byte[] uuid, byte[] key)
        {
            return MakeRequest(scripthash, request_read, uuid, key, value_null);
        }

        private static BigInteger API_Update(byte[] scripthash, byte[] uuid, byte[] key, byte[] value)
        {
            return MakeRequest(scripthash, request_update, uuid, key, value);
        }

        private static BigInteger API_Delete(byte[] scripthash, byte[] uuid, byte[] key)
        {
            return MakeRequest(scripthash, request_delete, uuid, key, value_null);
        }

        private static bool API_Push(BigInteger req_id, byte[] value)
        {
            if (!Runtime.CheckWitness(Admin_Address))
            {
                return false;
            }

            var base_key = req_id.AsByteArray();
            var req_result_key = request_key_prefix.Concat(base_key);
            Storage.Put(Storage.CurrentContext, req_result_key, value);

            return true;
        }

        private static byte[] API_Pull(BigInteger req_id)
        {
            var base_key = req_id.AsByteArray();
            var req_result_key = request_key_prefix.Concat(base_key);
            return Storage.Get(Storage.CurrentContext, req_result_key);
        }

        private static BigInteger MakeRequest(byte[] scriptHash, byte[] operation, byte[] uiid, byte[] key, byte[] value)
        {
            if (!Runtime.CheckWitness(scriptHash))
            {
                return 0;
            }

            if (operation.Length != 1)
            {
                return 0;
            }

            if (uiid.Length <= 0)
            {
                return 0;
            }

            if (key.Length <= 0)
            {
                return 0;
            }

            var req_count = Storage.Get(Storage.CurrentContext, request_count_prefix).AsBigInteger();
            req_count = req_count + 1;

            var base_key = req_count.AsByteArray();

            var req_owner_key = request_owner_prefix.Concat(base_key);
            Storage.Put(Storage.CurrentContext, req_owner_key, scriptHash);

            var req_key_key = request_key_prefix.Concat(base_key);
            Storage.Put(Storage.CurrentContext, req_key_key, operation.Concat(key));

            var req_uuid_key = request_key_prefix.Concat(base_key);
            Storage.Put(Storage.CurrentContext, req_uuid_key, uiid);

            if (value.AsBigInteger() != 0)
            {
                var req_val_key = request_value_prefix.Concat(base_key);
                Storage.Put(Storage.CurrentContext, req_val_key, value);
            }

            return req_count;
        }
    }
}
