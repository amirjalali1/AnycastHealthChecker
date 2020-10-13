using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AnycastHealthMonitor
{
    [DisallowConcurrentExecution]
    internal class HealthMonitorJob : IJob
    {
        private readonly ILogger<HealthMonitorJob> _logger;
        private readonly IHealthController _healthController;

        public HealthMonitorJob(IHealthController healthController, ILogger<HealthMonitorJob> logger)
        {
            _logger = logger;
            _healthController = healthController;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Executed in {DateTimeOffset.Now}");

            _healthController.Do();

            return Task.CompletedTask;
        }
    }
}
