using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Farakav.QuartzWorkerService.Internals
{
    [DisallowConcurrentExecution]
    internal class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        public QuartzJobRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                if (scope.ServiceProvider.GetRequiredService(context.JobDetail.JobType) is IJob job)
                    await job.Execute(context);
            }
        }
    }
}