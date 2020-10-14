using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnycastHealthMonitor.HealthChecker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace AnycastHealthMonitor
{
    [DisallowConcurrentExecution]
    internal class HealthMonitorJob : IJob
    {
        private readonly ILogger<HealthMonitorJob> _logger;
        private readonly IEnumerable<IHealthChecker> _healthCheckers;
        private readonly IHealthyAdvertiser _healthyAdvertiser;

        public HealthMonitorJob(IEnumerable<IHealthChecker> healthCheckers,
            IHealthyAdvertiser healthyAdvertiser,
            ILogger<HealthMonitorJob> logger)
        {
            _logger = logger;
            _healthCheckers = healthCheckers;
            _healthyAdvertiser = healthyAdvertiser;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Executed in {DateTimeOffset.Now}");

            var isHealthy = true;

            foreach (var healthChecker in _healthCheckers)
            {
                isHealthy &= healthChecker.IsHealthy();
            }

            if (isHealthy)
            {
                _logger.LogInformation("The system is Healthy");
                _healthyAdvertiser.AdvertiseHealthy();
            }
            else
            {
                _logger.LogWarning("The system is under pressure");
                _healthyAdvertiser.AdvertiseUnhealthy();
            }

            return Task.CompletedTask;
        }
    }
}
