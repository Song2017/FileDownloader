using System;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Models;
using Common.Utils;

namespace Services.Infrastructure
{
    public class CustomeCache : ICustomeCache
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;
        private static int Retry { set; get; } = 5;

        public CustomeCache(ILoggerFactory logger, IDistributedCache cache, IConfiguration config)
        {
            _cache = cache;
            _logger = logger.CreateLogger<CustomeCache>();
            Retry = config.GetAppSetting("RedisRetry").ToInt();
        }

        public async void SetStringAsync(string key, string value, DistributedCacheEntryOptions options)
        {
            if (!Enable()) return;

            try
            {
               await _cache.SetAsync(key, Encoding.UTF8.GetBytes(value), options);
            }
            catch (RedisConnectionException redisEx)
            {
                Retry--;
                _logger.LogError($"Cache Not Open: {redisEx.Message}");
            }
            catch (Exception ex)
            {
                Retry--;
                _logger.LogError($"Cache Error: {ex.Message}");
            }
        }

        public async Task<string> GetStringAsync(string key)
        {
            if (!Enable()) return null;

            try
            {
                var result = await _cache.GetAsync(key);
                return result != null ? Encoding.UTF8.GetString(result, 0, result.Length) : null;
            }
            catch (RedisConnectionException redisEx)
            {
                Retry--;
                _logger.LogError($"Cache Not Open: {redisEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Retry--;
                _logger.LogError($"Cache Error: {ex.Message}");
                return null;
            }
        }

        public bool Enable()
        {
            return Retry > 0;
        }

        public string GetCacheKey(Data valve)
        {
            if (valve == null)
                return null;

            // token(tenant|user) owner plant tag ser
            return $"ValveKey_{valve.Token}_{valve.OwnerName}_{valve.PlantLocation}" +
                   $"_{valve.TagNumber}_{valve.SerialNumber}_{valve.ValveTable}_{valve.FileType}";
        }


    }

}
