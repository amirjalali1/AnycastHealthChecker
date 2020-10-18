namespace AnycastHealthMonitor.SnapshotManagers
{
    public interface INetworkSnapshotManager
    {
        SnapshotResponse Take(string interfaceName);
    }
}
