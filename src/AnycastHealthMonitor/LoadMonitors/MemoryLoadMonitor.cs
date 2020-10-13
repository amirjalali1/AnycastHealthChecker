using System.Diagnostics;

namespace AnycastHealthMonitor.LoadMonitors
{
    public class MemoryLoadMonitor : ILoadMonitor
    {
        public MonitoredType MonitoredType => MonitoredType.Memory;

        public LoadReport GetLoadReport()
        {
            var output = "";

            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"free | grep Mem | awk '{print $3/$2 * 100.0}'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            if (float.TryParse(output.Trim(), out var percentageInUsedMemory))
            {
                return LoadReport.Success(percentageInUsedMemory);
            }

            return LoadReport.Fail("Failed to read info");
        }
    }
}
