using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace AnycastHealthMonitor.SnapshotManagers
{
    public class NetworkSnapshotManager : INetworkSnapshotManager
    {
        private readonly ILogger<NetworkSnapshotManager> _logger;

        public NetworkSnapshotManager(ILogger<NetworkSnapshotManager> logger)
        {
            _logger = logger;
        }

        public SnapshotResponse Take(string interfaceName, float interfaceCapacity)
        {
            var output = "";

            var arguments = $"-c \"ifstat -b -i {interfaceName}\"";

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
                var percentage = ((inTraffic + outTraffic) / (float)interfaceCapacity) * 100;

                //_logger.LogInformation($"In: {inTraffic}, Out: {outTraffic}, Capacity: {interfaceCapacity}, Percentage: {percentage}");

                return SnapshotResponse.Success(percentage);
            }

            return SnapshotResponse.Fail("Failed to read info");
        }
    }
}
