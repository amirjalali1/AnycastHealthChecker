using AnycastHealthMonitor.Settings;
using AnycastHealthMonitor.SnapshotManagers;
using Microsoft.Extensions.Options;
using System.Linq;

namespace AnycastHealthMonitor.HealthChecker
{
    public class NetworkHealthChecker : IHealthChecker
    {
        private readonly INetworkSnapshotManager _snapshotManager;
        private readonly NetworkSettings _networkSettings;
        private readonly IHealthyStore _healthyStore;

        public NetworkHealthChecker(IHealthyStore healthyStore, 
            INetworkSnapshotManager snapshotManager,
            IOptions<HealthCheckSettings> healthCheckSettingsOptionsAccessor)
        {
            _healthyStore = healthyStore;
            _snapshotManager = snapshotManager;
            _networkSettings = healthCheckSettingsOptionsAccessor.Value.Network;
        }

        public bool IsHealthy()
        {
            var snapshot = _snapshotManager.Take(_networkSettings.InterfaceName);

            const string key = "network";

            if (snapshot.Succeeded)
            {
                var snapshotIsHealthy = _networkSettings.Percentage > snapshot.Percentage;

                _healthyStore.AddHealthyStatus(key, snapshotIsHealthy);
            }

            var isUnhealty = _healthyStore.Collection(key)
                .Take(_networkSettings.UnhealthyCount)
                .All(e => e == false);

            return !isUnhealty;
        }
    }
}
