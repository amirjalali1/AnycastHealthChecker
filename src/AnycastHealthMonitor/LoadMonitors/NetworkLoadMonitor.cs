using System.Diagnostics;
using System.Linq;

namespace AnycastHealthMonitor.LoadMonitors
{
    public class NetworkLoadMonitor : ILoadMonitor
    {
        public MonitoredType MonitoredType => MonitoredType.Network;

        public LoadReport GetLoadReport()
        {
            var output = "";

            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"ifstat -b -i ens160\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                for (int i = 0; i < 3; i++)
                {
                    output = process.StandardOutput.ReadLine();
                }

                process.Kill();
            }

            var list = output.Split(" ")
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();

            if (float.TryParse(list[0], out float inTraffic) && float.TryParse(list[1], out float outTraffic))
            {
                return LoadReport.Success(inTraffic + outTraffic);
            }

            return LoadReport.Fail("Failed to read info");
        }
    }
}
