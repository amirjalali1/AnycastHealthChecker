using System;
using System.IO;
using System.Text;
using AnycastHealthMonitor.HealthChecker;
using AnycastHealthMonitor.Settings;
using Microsoft.Extensions.Options;

namespace AnycastHealthMonitor
{
    public class HealthyAdvertiser : IHealthyAdvertiser
    {
        private readonly AdvertiseSettings _advertiseSettings;

        public HealthyAdvertiser(IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _advertiseSettings = healthCheckSettingsOptionsAccessor.Value.Advertise;
        }

        public void AdvertiseHealthy()
        {
            using StreamWriter writer = new StreamWriter(_advertiseSettings.AnycastFilePath, false);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("define ACAST_PS_ADVERTISE =");
            sb.AppendLine("    [");
            sb.AppendLine($"        {_advertiseSettings.HealthyIp}");
            sb.AppendLine("    ];");

            writer.Write(sb.ToString());
        }

        public void AdvertiseUnhealthy()
        {
            using StreamWriter writer = new StreamWriter(_advertiseSettings.AnycastFilePath, false);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("define ACAST_PS_ADVERTISE =");
            sb.AppendLine("    [");
            sb.AppendLine($"        {_advertiseSettings.UnhealthyIp}");
            sb.AppendLine("    ];");

            writer.Write(sb.ToString());
        }
    }
}
