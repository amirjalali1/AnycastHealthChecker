namespace AnycastHealthMonitor.SnapshotManagers
{
    public class SnapshotResponse
    {
        public bool Succeeded { get; set; }

        public bool Failed { get; set; }

        public string FailedMessage { get; set; }

        public float Percentage { get; set; }

        public static SnapshotResponse Success(float percentage) => new SnapshotResponse() 
        {
            Succeeded = true,
            Percentage = percentage
        };

        public static SnapshotResponse Fail(string message) => new SnapshotResponse()
        {
            Failed = true,
            FailedMessage = message
        };
    }
}
