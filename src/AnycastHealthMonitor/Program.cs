using System;
using AnycastHealthMonitor.HealthChecker;
using AnycastHealthMonitor.Settings;
using AnycastHealthMonitor.SnapshotManagers;
using Farakav.QuartzWorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AnycastHealthMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();

            Log.Logger = CreateSerilogLogger(configuration);

            try
            {
                Log.Information("Configuring web host ...");

                var host = CreateHostBuilder(args).UseWindowsService().Build();

                Log.Information("Starting web host ...");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly !");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            var configuration = hostBuilderContext.Configuration;

            services.Configure<HealthCheckSettings>(configuration.GetSection("HealthCheckSettings"));

            services.AddSingleton<IHealthyStore, HealthyStore>();
            services.AddSingleton<IHealthyAdvertiser, HealthyAdvertiser>();

            services.AddTransient<IProcessorSnapshotManager, ProcessorSnapshotManager>();
            services.AddTransient<IMemorySnapshotManager, MemorySnapshotManager>();
            services.AddTransient<INetworkSnapshotManager, NetworkSnapshotManager>();

            services.AddTransient<IHealthChecker, NginxHealthChecker>();
            services.AddTransient<IHealthChecker, NginxRequestHealthChecker>();
            services.AddTransient<IHealthChecker, NetworkHealthChecker>();
            services.AddTransient<IHealthChecker, MemoryHealthChecker>();
            services.AddTransient<IHealthChecker, ProcessorHealthChecker>();

            services.AddHttpClient("nginx-api", c =>
            {
                c.Timeout = new TimeSpan(0, 0, 3);
            });


            services.AddJobScheduler(configuration.GetSection("JobSettings"))
                .AddJob<HealthMonitorJob>()
                .Configure();
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static ILogger CreateSerilogLogger(IConfiguration config)
        {
            return new LoggerConfiguration()
                           .ReadFrom.Configuration(config)
                           .Enrich.FromLogContext()
                           .Enrich.WithMachineName()
                           .Enrich.WithThreadId()
                           .CreateLogger();
        }
    }
}
