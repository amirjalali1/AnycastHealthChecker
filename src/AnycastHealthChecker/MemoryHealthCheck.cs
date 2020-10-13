using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace AnycastHealthChecker
{
    public interface IHealthCheck
    {
        bool IsHealthy();
    }

    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly ILogger<MemoryHealthCheck> _logger;

        public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
        {
            _logger = logger;
        }

        public bool IsHealthy()
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

            if(float.TryParse(output.Trim(), out var percentageInUsedMemory))
            {
                _logger.LogInformation($"In Used Memory {percentageInUsedMemory} %");

                if(percentageInUsedMemory > 95)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class CpuHealthCheck : IHealthCheck
    {
        private readonly ILogger<MemoryHealthCheck> _logger;

        public CpuHealthCheck(ILogger<MemoryHealthCheck> logger)
        {
            _logger = logger;
        }

        public bool IsHealthy()
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
                _logger.LogInformation($"In Used Cpu {percentageInUsedCpu} %");

                if (percentageInUsedCpu > 95)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class NetworkHealthCheck : IHealthCheck
    {
        private readonly ILogger<MemoryHealthCheck> _logger;

        public NetworkHealthCheck(ILogger<MemoryHealthCheck> logger)
        {
            _logger = logger;
        }

        public bool IsHealthy()
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

            float.TryParse(list[0], out float inTraffic);
            float.TryParse(list[1], out float outTraffic);

            _logger.LogInformation($"Network in: {inTraffic}, out: {outTraffic}");

            var tenG = Math.Pow(10, 7);

            if(inTraffic > tenG * .9)
            {
                return false;
            }

            if (outTraffic > tenG * .9)
            {
                return false;
            }

            return true;
        }
    }
}