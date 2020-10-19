using AnycastHealthMonitor.Settings;
using AnycastHealthMonitor.SnapshotManagers;
using Microsoft.Extensions.Options;
using System.Linq;

namespace AnycastHealthMonitor.HealthChecker
{
    public class ProcessorHealthChecker : IHealthChecker
    {
        private readonly IProcessorSnapshotManager _snapshotManager;
        private readonly ProcessorSettings _processorSettings;
        private readonly IHealthyStore _healthyStore;

        public ProcessorHealthChecker(IHealthyStore healthyStore, 
            IProcessorSnapshotManager snapshotManager,
            IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _healthyStore = healthyStore;
            _snapshotManager = snapshotManager;
            _processorSettings = healthCheckSettingsOptionsAccessor.Value.Processor;
        }

        public bool IsHealthy()
        {
            var snapshot = _snapshotManager.Take();

            const string key = "processor";

            if (snapshot.Succeeded)
            {
                var snapshotIsHealthy = _processorSettings.Percentage > snapshot.Percentage;

                _healthyStore.AddHealthyStatus(key, snapshotIsHealthy);
            }

            var isUnhealty = _healthyStore.Collection(key)
                .Take(_processorSettings.UnhealthyCount)
                .All(e => e == false);

            return !isUnhealty;
        }
    }
}
