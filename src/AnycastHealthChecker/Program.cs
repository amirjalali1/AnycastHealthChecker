using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;

namespace AnycastHealthChecker
{
    public class Program
    {
 
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();

            Log.Logger = CreateSerilogLogger(configuration);

            try
            {
                Log.Error("Configuring web host ...");

                var host = CreateHostBuilder(args).Build();

                LogPackagesVersionInfo();

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

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();




            return builder.Build();
        }


        private static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
        {

              return new LoggerConfiguration()
                           .ReadFrom.Configuration(configuration)
                           .Enrich.FromLogContext()
                           .Enrich.WithMachineName()
                           .Enrich.WithThreadId()
                           .CreateLogger();
        }

        private static void LogPackagesVersionInfo()
        {
            var assemblies = new List<Assembly>();

            foreach (var dependencyName in typeof(Program).Assembly.GetReferencedAssemblies())
            {
                try
                {
                    // Try to load the referenced assembly...
                    assemblies.Add(Assembly.Load(dependencyName));
                }
                catch
                {
                    // Failed to load assembly. Skip it.
                }
            }

            var versionList = assemblies.Select(a => $"-{a.GetName().Name} - {GetVersion(a)}").OrderBy(value => value);

            Log.Logger.ForContext("PackageVersions", string.Join("\n", versionList)).Information("Package versions");
        }

        private static string GetVersion(Assembly assembly)
        {
            try
            {
                return $"{assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version} ({assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split()[0]})";
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            var configuration = hostBuilderContext.Configuration;

            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();

            // Add our job
            services.AddSingleton<CheckHealthJob>();
            services.AddTransient<IProcessorHealthCheck, ProcessorHealthCheck>();
            services.AddTransient<IMemoryCache, MemoryCache>();
            services.AddHttpClient<INginxHelathCheck, NginxHelathCheck>();


            services.AddSingleton(new JobScheduleSetting(
                                                      jobType: typeof(CheckHealthJob),
                                                      cronExpression: "0/5 * * * * ?"));// run every 5 seconds


        }




        public static IHostBuilder CreateHostBuilder(string[] args) =>

               Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(ConfigureServices);
        //Host.CreateDefaultBuilder(args)
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddHostedService<Worker>();
        //        });
    }
}
