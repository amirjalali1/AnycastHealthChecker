using System.Collections.Generic;
using System.Linq;
using AnycastHealthMonitor.HealthChecker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnycastHealthMonitor
{
    public class HealthController : IHealthController
    {
        private readonly IEnumerable<IHealthChecker> _healthCheckers;
        private readonly IHealthHistoryManager _healthHistoryManager;
        private readonly HealthCheckerSettings _healthCheckerSettings;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IEnumerable<IHealthChecker> healthCheckers, 
            IHealthHistoryManager healthHistoryManager, 
            IOptions<HealthCheckerSettings> healthCheckerSettingsOptionsAccessor,
            ILogger<HealthController> logger)
        {
            _healthCheckers = healthCheckers;
            _healthHistoryManager = healthHistoryManager;
            _healthCheckerSettings = healthCheckerSettingsOptionsAccessor.Value;
            _logger = logger;
        }

        public void Do()
        {
            var isHealthy = _healthCheckers.All(e => e.IsHealthy());
            
            _healthHistoryManager.AddHealthyStatus(isHealthy);

            var unhealtyCount = _healthHistoryManager.GetUnhealtyCount();

            _logger.LogInformation($"Unhealty Count: {unhealtyCount}, Settings.UnhealthyCount: {_healthCheckerSettings.UnhealthyCount}");

            if (_healthCheckerSettings.UnhealthyCount < unhealtyCount)
            {
                _logger.LogWarning("The system is under pressure");
            }
            else
            {
                _logger.LogInformation("The system is Healthy");
            }
        }
    }
}
