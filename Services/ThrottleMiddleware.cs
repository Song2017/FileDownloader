using Common.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Infrastructure;
using System.Threading.Tasks;

namespace Services
{
    public class ThrottleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICustomeCache _cache;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        // Only works with Cache
        public ThrottleMiddleware(RequestDelegate next, ICustomeCache cache, ILoggerFactory logger,
            IConfiguration config)
        {
            _next = next;
            _cache = cache;
            _logger = logger.CreateLogger<ThrottleMiddleware>();
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // add user log
            var request = context.Request;
            var authToken = request.Headers["Authorization"].ToString();
            if (!authToken.IsNullOrEmptyOrSpace() && _cache.Enable())
            {
                var token = authToken.GetToken();
                var count = _cache.GetStringAsync(authToken);
                if (!count.Result.IsNullOrEmptyOrSpace())
                {
                    var curCount = count.Result.ToInt() + 1;
                    if (curCount > _config.GetAppSetting("ThrottlePerUser").ToInt())
                    {
                        _logger.LogError($"Throttle: {authToken}");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Token has used more than " +
                               $"{_config.GetAppSetting("ThrottlePerUser")} times. Please login again.");
                        return;
                    }

                    _cache.SetStringAsync(authToken, curCount.ToStringEx(),
                        new DistributedCacheEntryOptions() { AbsoluteExpiration = token.ValidTo });
                }
                else
                {
                    _cache.SetStringAsync(authToken, "1",
                        new DistributedCacheEntryOptions() { AbsoluteExpiration = token.ValidTo });
                }
            }

            await _next(context);
        }

    }

    public static class CustomeMiddlewareExtensions
    {
        public static IApplicationBuilder UseThrottleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThrottleMiddleware>();
        }
    }
}
