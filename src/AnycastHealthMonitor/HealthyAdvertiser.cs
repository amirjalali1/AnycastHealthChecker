using System.Diagnostics;
using System.IO;
using System.Text;
using AnycastHealthMonitor.Settings;
using Microsoft.Extensions.Options;

namespace AnycastHealthMonitor
{
    public class HealthyAdvertiser : IHealthyAdvertiser
    {
        private readonly AdvertiseSettings _advertiseSettings;
        private bool? LatestIsHealthy = null;

        public HealthyAdvertiser(IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _advertiseSettings = healthCheckSettingsOptionsAccessor.Value.Advertise;
        }

        public void AdvertiseHealthy()
        {
            if (LatestIsHealthy == true)
            {
                return;
            }

            LatestIsHealthy = true;

            using StreamWriter writer = new StreamWriter(_advertiseSettings.AnycastFilePath, false);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("define ACAST_PS_ADVERTISE =");
            sb.AppendLine("    [");
            sb.AppendLine($"        {_advertiseSettings.HealthyIp}");
            sb.AppendLine("    ];");

            writer.Write(sb.ToString());

            ReconfigureBird();
        }

        public void AdvertiseUnhealthy()
        {
            if (LatestIsHealthy == false)
            {
                return;
            }

            LatestIsHealthy = false;

            using StreamWriter writer = new StreamWriter(_advertiseSettings.AnycastFilePath, false);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("define ACAST_PS_ADVERTISE =");
            sb.AppendLine("    [");
            sb.AppendLine($"        {_advertiseSettings.UnhealthyIp}");
            sb.AppendLine("    ];");

            writer.Write(sb.ToString());

            ReconfigureBird();
        }

        private void ReconfigureBird()
        {
            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"/usr/sbin/birdc configure\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                process.WaitForExit();
            }
        }
    }
}
