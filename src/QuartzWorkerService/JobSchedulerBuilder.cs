using Microsoft.Extensions.DependencyInjection;

namespace Farakav.QuartzWorkerService
{
    public class JobSchedulerBuilder
    {
        internal JobSchedulerBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}