using System.Threading.Tasks;

namespace Bluzelle.NEO.Sharp.Core
{
    public interface ISwarm
    {
        Task<bool> Create(string uuid, string key, byte[] value);
        Task<byte[]> Read(string uuid, string key);

        Task<bool> Update(string uuid, string key, byte[] value);
        Task<bool> Remove(string uuid, string key);
    }
}
