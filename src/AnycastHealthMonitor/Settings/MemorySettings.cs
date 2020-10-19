namespace AnycastHealthMonitor.Settings
{
    public class MemorySettings
    {
        public float Percentage { get; set; } = 90;
        public int UnhealthyCount { get; set; } = 10;
    }
}
