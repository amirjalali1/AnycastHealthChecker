namespace AnycastHealthMonitor.Settings
{
    public class NginxRequestSettings
    {
        public string Url { get; set; }

        public int UnhealthyCount { get; set; } = 10;

        public int ExpectedResponseTimeInMilliSecond { get; set; } = 50;
    }
}
