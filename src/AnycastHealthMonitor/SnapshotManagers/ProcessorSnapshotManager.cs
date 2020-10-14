using System.Diagnostics;

namespace AnycastHealthMonitor.SnapshotManagers
{
    public class ProcessorSnapshotManager : IProcessorSnapshotManager
    {
        public SnapshotResponse Take()
        {
            var output = "";

            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"awk '{u=$2+$4; t=$2+$4+$5; if (NR==1){u1=u; t1=t;} else print ($2+$4-u1) * 100 / (t-t1); }' <(grep 'cpu ' /proc/stat) <(sleep 1;grep 'cpu ' /proc/stat)\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            if (float.TryParse(output.Trim(), out var percentageInUsedCpu))
            {
                return SnapshotResponse.Success(percentageInUsedCpu);
            }

            return SnapshotResponse.Fail("Failed to read info");
        }
    }
}
