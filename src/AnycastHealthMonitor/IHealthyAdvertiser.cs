namespace AnycastHealthMonitor
{
    public interface IHealthyAdvertiser
    {
        void AdvertiseHealthy();
        void AdvertiseUnhealthy();
    }
}
