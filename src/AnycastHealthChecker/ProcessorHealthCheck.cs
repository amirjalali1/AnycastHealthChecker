using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace AnycastHealthChecker
{
    public class ProcessorHealthCheck : IProcessorHealthCheck
    {
        public ILogger<ProcessorHealthCheck> Logger { get; }

        private class CpuMetrics
        {
            public string RawData { get; set; }
            public float LoadPercentage { get; set; }
        }


        public ProcessorHealthCheck(ILogger<ProcessorHealthCheck> logger)
        {
            Logger = logger;
        }

        public bool CheckHealth()
        {
            //var isWin = !IsUnix();

            //if (isWin)
            //{
            //    return GetWindowsBasedMetrics();
            //}
            CpuMetrics infoMpStat = GetUnixCpuInfoWithMpStat();
            CpuMetrics infoTop = GetUnixCpuInfoWithTop();
            var message = $", CPU Usage: {infoMpStat.LoadPercentage:F}%";

            Logger.LogCritical($"infoMpStat {infoMpStat.LoadPercentage} raw data {infoMpStat.RawData}");
            Logger.LogError($"infoTop {infoTop.LoadPercentage} raw data {infoTop.RawData}");
            return true;
        }


        //private Task<HealthCheckResult> GetWindowsBasedMetrics()
        //{
        //    var status = HealthStatus.Healthy;
        //    CpuMetrics info = GetWinCpuInfo();

        //    var message = $"{(status != HealthStatus.Healthy ? _healthCheckConfiguration.Unhealthy : _healthCheckConfiguration.Healthy)}" +
        //                  $", {_healthCheckConfiguration.Key}" +
        //                  $", CPU Usage: {info.LoadPercentage:F}%";

        //    var data = new Dictionary<string, object>
        //            {
        //                {nameof(info.LoadPercentage), info.LoadPercentage},
        //                {nameof(info.RawData), info.RawData}
        //            };

        //    return Task.FromResult(new HealthCheckResult(status, message, null, data));
        //}

        private static CpuMetrics GetWinCpuInfo()
        {
            var output = "";

            var info = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "CPU list full",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
            }

            var dict = new Dictionary<string, string>();
            var lines = output.Split("\n").ToList();

            lines.ForEach(line =>
            {
                var data = line.Split("=");
                dict.Add(data[0], data[1]);
            });

            float.TryParse(dict["LoadPercentage"], out var load);

            return new CpuMetrics { LoadPercentage = load, RawData = output };
        }

        private static CpuMetrics GetUnixCpuInfoWithTop()
        {
            var output = "";

            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"top -b -n 5 -d.2 | grep \"Cpu\" |  awk 'NR==3{ print($2)}'\"", // top -bn 1 | grep "^ " | awk '{ printf("%-8s  %-8s  %-8s\n", $9, $10, $12); }' | head -n 5
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            float.TryParse(output, out var value);
            var total = value;

            return new CpuMetrics { LoadPercentage = total, RawData = output };

        }

        private CpuMetrics GetUnixCpuInfoWithMpStat()
        {
            var output = "";

            var info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"mpstat -P ALL 1 1\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            var dict = new List<string>();
            var lines = output.Trim().Split("\n");
            var lineIndicatingAll = lines.Skip(3).Take(1).FirstOrDefault();

            if (lineIndicatingAll == null)
            {
                return new CpuMetrics();
            }

            var data = lineIndicatingAll.Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

            float total = 0;

            for (var index = 0; index < data.Count - 1; index++)
            {
                float.TryParse(data[index], out var value);
                total += value;
            };

            return new CpuMetrics { LoadPercentage = total, RawData = output };

        }

        private static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

    }
}
