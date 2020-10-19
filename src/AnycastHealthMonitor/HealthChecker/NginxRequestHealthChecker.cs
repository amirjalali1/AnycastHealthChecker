using AnycastHealthMonitor.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace AnycastHealthMonitor.HealthChecker
{
    public class NginxRequestHealthChecker : IHealthChecker
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NginxRequestSettings _settings;
        private readonly IHealthyStore _healthyStore;

        public NginxRequestHealthChecker(IHttpClientFactory httpClientFactory, 
            IHealthyStore healthyStore, 
            IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _healthyStore = healthyStore;
            _settings = healthCheckSettingsOptionsAccessor.Value.NginxRequest;
        }

        public bool IsHealthy()
        {
            var stopwatch  = new Stopwatch();

            const string key = "nginx-request";

            bool requestIsHealthy = false;

            try
            {
                stopwatch.Start();

                var httpClient = _httpClientFactory.CreateClient("nginx-api");

                var response = httpClient.GetAsync(_settings.Url).GetAwaiter().GetResult();

                stopwatch.Stop();

                requestIsHealthy = response.IsSuccessStatusCode &&
                    _settings.ExpectedResponseTimeInMilliSecond > stopwatch.ElapsedMilliseconds;
            }
            catch (System.Exception)
            {
                requestIsHealthy = false;
            }

            _healthyStore.AddHealthyStatus(key, requestIsHealthy);

            var isUnhealty = _healthyStore.Collection(key)
                    .Take(_settings.UnhealthyCount)
                    .All(e => e == false);

            return !isUnhealty;
        }
    }
}
