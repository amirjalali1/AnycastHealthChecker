namespace AnycastHealthMonitor.Settings
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
}
