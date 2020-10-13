namespace AnycastHealthMonitor.LoadMonitors
{
    public class LoadReport
    {
        public bool Succeeded { get; set; }

        public bool Failed { get; set; }

        public string FailedMessage { get; set; }

        public float Percentage { get; set; }

        public static LoadReport Success(float percentage) => new LoadReport() 
        {
            Succeeded = true,
            Percentage = percentage
        };

        public static LoadReport Fail(string message) => new LoadReport()
        {
            Failed = true,
            FailedMessage = message
        };
    }
}
