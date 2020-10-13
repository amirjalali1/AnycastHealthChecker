using System.Diagnostics;

namespace AnycastHealthMonitor.HealthChecker
{
    public class NginxHealthChecker : IHealthChecker
    {
        public bool IsHealthy()
        {
            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"systemctl is-active nginx\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(info);

            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return output.Trim().Equals("active");
        }
    }
}
