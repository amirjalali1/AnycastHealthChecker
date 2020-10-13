namespace AnycastHealthMonitor
{
    public interface IHealthHistoryManager
    {
        void AddHealthyStatus(bool isHealthy);

        int GetUnhealtyCount();
    }
}
