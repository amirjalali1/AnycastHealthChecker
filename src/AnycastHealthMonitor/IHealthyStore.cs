using System.Collections.Generic;

namespace AnycastHealthMonitor
{
    public interface IHealthyStore
    {
        void AddHealthyStatus(string key, bool isHealthy);

        IReadOnlyCollection<bool> Collection(string key);
    }
}
