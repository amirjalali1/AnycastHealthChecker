using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AnycastHealthMonitor.SnapshotManagers
{
    public class NetworkSnapshotManager : INetworkSnapshotManager
    {
        private readonly ILogger<NetworkSnapshotManager> _logger;

        public NetworkSnapshotManager(ILogger<NetworkSnapshotManager> logger)
        {
            _logger = logger;
        }

        public SnapshotResponse Take(string interfaceName)
        {
            var output = "";

            var arguments = string.Format("-c \"awk '{{if (NR==1){{r1=$1;}} else if (NR==2){{r2=$1;}} else {{print (((r2-r1)*8/1024/1024)*100/$1);}}}}' " +
                "<(cat /proc/net/dev|grep {0} | awk '{{print($2 +$10)}}') " +
                "<(sleep 1;cat /proc/net/dev|grep {0} | awk '{{print($2 +$10)}}') " +
                "<(cat /sys/class/net/{0}/speed | awk '{{print $1}}')\"", interfaceName);

            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            if (float.TryParse(output.Trim(), out var percentage))
            {
                return SnapshotResponse.Success(percentage);
            }

            return SnapshotResponse.Fail("Failed to read info");
        }
    }
}
