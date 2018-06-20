using System.Threading.Tasks;

namespace Bluzelle.NEO.Sharp.Core
{
    public interface ISwarm
    {
        Task<bool> Create(string uuid, string key, string value);
        Task<string> Read(string uuid, string key);

        Task<bool> Update(string uuid, string key, string value);
        Task<bool> Delete(string uuid, string key);
    }
}
