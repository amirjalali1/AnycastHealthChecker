namespace AnycastHealthMonitor.SnapshotManagers
{
    public interface ISnapshotManager
    {
        SnapshotResponse Take();
    }
}
