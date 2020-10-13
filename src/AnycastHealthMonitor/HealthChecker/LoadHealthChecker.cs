using AnycastHealthMonitor.LoadMonitors;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace AnycastHealthMonitor.HealthChecker
{
    public class LoadHealthChecker : IHealthChecker
    {
        private readonly IEnumerable<ILoadMonitor> _loadMonitors;
        private readonly HealthCheckerSettings _healthCheckerSettings; 

        public LoadHealthChecker(IEnumerable<ILoadMonitor> loadMonitors, 
            IOptions<HealthCheckerSettings> healthCheckerSettingsOptionsAccessor)
        {
            _loadMonitors = loadMonitors;
            _healthCheckerSettings = healthCheckerSettingsOptionsAccessor.Value;
        }

        public bool IsHealthy()
        {
            foreach (var loadMonitor in _loadMonitors)
            {
                var loadReport = loadMonitor.GetLoadReport();

                if (loadReport.Succeeded)
                {
                    var found = _healthCheckerSettings.Monitors.FirstOrDefault(e => e.MonitoredType == loadMonitor.MonitoredType);

                    if(found.Percentage < loadReport.Percentage)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
