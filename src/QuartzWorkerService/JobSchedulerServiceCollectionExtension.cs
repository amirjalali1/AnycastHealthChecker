using System.Linq;
using Farakav.QuartzWorkerService.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Farakav.QuartzWorkerService
{
    public static class JobSchedulerServiceCollectionExtension
    {
        private static JobSettings _jobSettings;

        public static JobSchedulerBuilder AddJobScheduler(this IServiceCollection services, IConfigurationSection configuration)
        {
            _jobSettings = new JobSettings();

            configuration.Bind(_jobSettings);

            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<QuartzJobRunner>();
            services.AddHostedService<QuartzBackgroundService>();

            return new JobSchedulerBuilder(services);
        }

        public static JobSchedulerBuilder AddJob<TJob>(this JobSchedulerBuilder builder) where TJob : class, IJob
        {
            var jobType = typeof(TJob);

            var cronExpression = _jobSettings.Jobs.Where(e => e.JobType.Equals(jobType.Name))
                .Select(e => e.CronExpression)
                .First();

            builder.Services.AddScoped(jobType);
            builder.Services.AddSingleton(new JobSchedule(jobType: jobType, cronExpression: cronExpression));

            return builder;
        }

        public static void Configure(this JobSchedulerBuilder builder)
        {
        }
    }
}
