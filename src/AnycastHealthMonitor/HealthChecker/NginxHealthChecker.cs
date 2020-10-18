using AnycastHealthMonitor.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Linq;

namespace AnycastHealthMonitor.HealthChecker
{
    public class NginxHealthChecker : IHealthChecker
    {
        private readonly IHealthyStore _healthyStore;
        private readonly NginxSettings _nginxSettings;

        public NginxHealthChecker(IHealthyStore healthyStore, 
            IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _healthyStore = healthyStore;
            _nginxSettings = healthCheckSettingsOptionsAccessor.Value.Nginx;
        }

        public bool IsHealthy()
        {
            var nginxIsActive = IsActiveNginx();

            const string key = "nginx";

            _healthyStore.AddHealthyStatus(key, nginxIsActive);

            var isUnhealty = _healthyStore.Collection(key)
                .Take(_nginxSettings.HealthyCount)
                .Any(e => e == false);

            return !isUnhealty;
        }

        private bool IsActiveNginx()
        {
            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"systemctl is-active nginx\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var process = Process.Start(info);
            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return output.Trim().Equals("active");
        }
    }
}
