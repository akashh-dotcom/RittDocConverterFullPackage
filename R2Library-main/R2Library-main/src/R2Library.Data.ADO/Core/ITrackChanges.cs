namespace R2Library.Data.ADO.Core
{
    public interface ITrackChanges
    {
        string TrackChangesHash { get; }

        void SetTrackChangesHash();

        bool IsDirty();
    }
}