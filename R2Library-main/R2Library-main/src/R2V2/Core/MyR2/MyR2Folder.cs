#region

using System;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class MyR2Folder : IMyR2Folder
    {
        public MyR2Folder(IMyR2Folder myR2Folder)
        {
            Id = myR2Folder.Id;
            Type = myR2Folder.Type;
            UserId = myR2Folder.UserId;
            FolderName = myR2Folder.FolderName;
            DefaultFolder = myR2Folder.DefaultFolder;
            RecordStatus = myR2Folder.RecordStatus;
        }

        public int Id { get; private set; }
        public MyR2Type Type { get; private set; }
        public int UserId { get; private set; }
        public string FolderName { get; private set; }
        public bool DefaultFolder { get; private set; }
        public bool RecordStatus { get; private set; }
    }
}