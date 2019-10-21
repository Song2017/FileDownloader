using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Models;

namespace Services.Infrastructure
{
    public interface ICustomeCache
    {
        void SetStringAsync(string key, string value, DistributedCacheEntryOptions options);

        string GetCacheKey(Data valve);

        Task<string> GetStringAsync(string key);

        bool Enable();
    }
}
