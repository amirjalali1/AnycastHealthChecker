using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace AnycastHealthChecker
{
    public class NginxHelathCheck : INginxHelathCheck
    {
        private  ILogger<NginxHelathCheck> Logger { get; }
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public NginxHelathCheck(ILogger<NginxHelathCheck> logger, HttpClient httpClient, IMemoryCache memoryCache)
        {
            Logger = logger;
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public async Task<bool> IsHealthy()
        {
            Stopwatch watch = new Stopwatch();
            var counter = _memoryCache.Get<int>("countiing");
            if(counter==0)
            {
                _memoryCache.Set("countiing", 1);
            }
            else
            {
                _memoryCache.Set("countiing", ++counter);
            }
         
            watch.Start();
            var response = await _httpClient.GetAsync("http://localhost:8020/nginx-health");
            var contentResult = await response.Content.ReadAsStringAsync();
            watch.Stop();
            Logger.LogCritical($"Elapsed time {watch.ElapsedMilliseconds} ms");
            Logger.LogCritical($"contentResult {contentResult}");
            Logger.LogCritical($"counter {counter}");
            return true;
        }

    }
}
