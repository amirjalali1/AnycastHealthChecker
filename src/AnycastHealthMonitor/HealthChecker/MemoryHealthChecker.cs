using AnycastHealthMonitor.SnapshotManagers;
using Microsoft.Extensions.Options;
using System.Linq;

namespace AnycastHealthMonitor.HealthChecker
{
    public class MemoryHealthChecker : IHealthChecker
    {
        private readonly IMemorySnapshotManager _snapshotManager;
        private readonly MemorySettings _memorySettings;
        private readonly IHealthyStore _healthyStore;

        public MemoryHealthChecker(IHealthyStore healthyStore, 
            IMemorySnapshotManager snapshotManager,
            IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _snapshotManager = snapshotManager;
            _memorySettings = healthCheckSettingsOptionsAccessor.Value.Memory;
            _healthyStore = healthyStore;
        }

        public bool IsHealthy()
        {
            var snapshot = _snapshotManager.Take();

            const string key = "memory";

            if (snapshot.Succeeded)
            {
                var isHealthy = _memorySettings.Percentage > snapshot.Percentage;

                _healthyStore.AddHealthyStatus(key, isHealthy);
            }

            var isUnhealty = _healthyStore.Collection(key)
                .Take(_memorySettings.UnhealthyCount)
                .All(e => e == false);

            return !isUnhealty;
        }
    }
}
