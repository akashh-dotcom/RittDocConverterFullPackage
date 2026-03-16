namespace R2V2.Core.MyR2
{
    public interface IMyR2Folder
    {
        int Id { get; }
        MyR2Type Type { get; }
        int UserId { get; }
        string FolderName { get; }
        bool DefaultFolder { get; }
        bool RecordStatus { get; }
    }
}