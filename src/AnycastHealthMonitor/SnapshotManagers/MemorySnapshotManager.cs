using System.Diagnostics;

namespace AnycastHealthMonitor.SnapshotManagers
{
    public class MemorySnapshotManager : IMemorySnapshotManager
    {
        public SnapshotResponse Take()
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
                return SnapshotResponse.Success(percentageInUsedMemory);
            }

            return SnapshotResponse.Fail("Failed to read info");
        }
    }
}
