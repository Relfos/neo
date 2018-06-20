using Bluzelle.NEO.Sharp.Core;
using Neo.Lux.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bluzelle.NEO.Tests
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public class TestSwarm : ISwarm
    {
        private Dictionary<string, string> storage = new Dictionary<string, string>();

        public async Task<bool> Create(string uuid, string key, string value)
        {
            if (storage.ContainsKey(key))
            {
                return false;
            }

            storage[key] = value;
            return true;
        }

        public async Task<string> Read(string uuid, string key)
        {
            return storage.ContainsKey(key) ? storage[key] : null;
        }

        public async Task<bool> Delete(string uuid, string key)
        {
            if (storage.ContainsKey(key))
            {
                storage.Remove(key);
                return true;
            }

            return false;
        }

        public async Task<bool> Update(string uuid, string key, string value)
        {
            if (storage.ContainsKey(key))
            {
                storage[key] = value;
                return true;
            }

            return false;
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
