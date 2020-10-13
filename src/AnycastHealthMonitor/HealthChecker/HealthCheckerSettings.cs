using AnycastHealthMonitor.LoadMonitors;
using System.Collections.Generic;

namespace AnycastHealthMonitor.HealthChecker
{
    public class HealthCheckerSettings
    {
        public int UnhealthyCount { get; set; }
        public List<Monitor> Monitors { get; set; }

        public class Monitor
        {
            public MonitoredType MonitoredType { get; set; }
            public float Percentage { get; set; }
        }
    }
}
