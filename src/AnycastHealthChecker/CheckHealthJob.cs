using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;


namespace AnycastHealthChecker
{
    [DisallowConcurrentExecution]
    public  class CheckHealthJob : IJob
    {
        private readonly ILogger<CheckHealthJob> _logger;
        private readonly IProcessorHealthCheck _processorHealthCheck;
        private readonly INginxHelathCheck _nginxHelathCheck;

        public CheckHealthJob(ILogger<CheckHealthJob> logger, IProcessorHealthCheck processorHealthCheck, INginxHelathCheck nginxHelathCheck)
        {
            _logger = logger;
            this._processorHealthCheck = processorHealthCheck;
            this._nginxHelathCheck = nginxHelathCheck;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogCritical("Hello world!");

            _processorHealthCheck.CheckHealth();
              _nginxHelathCheck.IsHealthy().ConfigureAwait(true);
            //ram
            //network 10g
            //nginx is available


            return Task.CompletedTask;
        }
    }
}
