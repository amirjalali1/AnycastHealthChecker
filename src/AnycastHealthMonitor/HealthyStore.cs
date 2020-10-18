using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnycastHealthMonitor
{
    public class HealthyStore : IHealthyStore
    {
        private readonly Dictionary<string, Queue<bool>> QueueDictionary;
        private readonly ILogger<HealthyStore> _logger;

        public HealthyStore(ILogger<HealthyStore> logger)
        {
            QueueDictionary = new Dictionary<string, Queue<bool>>();
            _logger = logger;
        }

        public void AddHealthyStatus(string key, bool isHealthy)
        {
            var queue = GetQueue(key);

            queue.Enqueue(isHealthy);

            if (queue.Count > 100)
            {
                queue.Dequeue();
            }

            var msg = string.Join(' ', queue);

            _logger.LogInformation($"{key}: {msg}");
        }

        public IReadOnlyCollection<bool> Collection(string key)
        {
            var collection = GetQueue(key);

            collection.Reverse();

            return collection;
        }

        private Queue<bool> GetQueue(string key)
        {
            if (!QueueDictionary.TryGetValue(key, out var queue))
            {
                queue = new Queue<bool>();

                QueueDictionary.Add(key, queue);
            }

            return queue;
        }
    }
}
