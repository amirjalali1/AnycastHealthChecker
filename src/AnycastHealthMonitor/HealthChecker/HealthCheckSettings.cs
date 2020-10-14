namespace AnycastHealthMonitor.HealthChecker
{
    public class HealthCheckSettings
    {
        public AdvertiseSettings Advertise { get; set; }
        public NetworkSettings Network { get; set; }
        public MemorySettings Memory { get; set; }
        public ProcessorSettings Processor { get; set; }
        public NginxSettings Nginx { get; set; }
        public NginxRequestSettings NginxRequest { get; set; }
    }

    public class AdvertiseSettings
    {
        public string Ip { get; set; }
        public string AnycastFilePath { get; set; }
    }

    public class NetworkSettings
    {
        public string InterfaceName { get; set; }
        public InterfaceCapacity InterfaceCapacity { get; set; } = InterfaceCapacity.TenG;
        public float Percentage { get; set; } = 90;
        public int UnhealthyCount { get; set; } = 10;
    }

    public class NginxSettings
    {
        public int HealthyCount { get; set; } = 3;
    }

    public class NginxRequestSettings
    {
        public string Url { get; set; }

        public int UnhealthyCount { get; set; } = 10;

        public int ExpectedResponseTimeInMilliSecond { get; set; } = 50;
    }

    public class MemorySettings
    {
        public float Percentage { get; set; } = 90;
        public int UnhealthyCount { get; set; } = 10;
    }

    public class ProcessorSettings
    {
        public float Percentage { get; set; } = 90;
        public int UnhealthyCount { get; set; } = 10;
    }

    public enum InterfaceCapacity
    {
        OneG = 1,
        TenG = 10,
        TwoTenG = 20
    }
}
