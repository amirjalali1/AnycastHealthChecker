namespace AnycastHealthMonitor.LoadMonitors
{
    public interface ILoadMonitor
    {
        MonitoredType MonitoredType { get; }

        LoadReport GetLoadReport();
    }
}
