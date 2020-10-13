using System.Collections.Generic;
using System.Linq;

namespace AnycastHealthMonitor
{
    public class HealthHistoryManager : IHealthHistoryManager
    {
        private readonly Queue<bool> HealthQueue;

        public HealthHistoryManager()
        {
            HealthQueue = new Queue<bool>();
        }

        public void AddHealthyStatus(bool isHealthy)
        {
            HealthQueue.Enqueue(isHealthy);

            if (HealthQueue.Count > 10)
            {
                HealthQueue.Dequeue();
            }
        }

        public int GetUnhealtyCount() => HealthQueue.Where(e => e == false).Count();
    }
}
