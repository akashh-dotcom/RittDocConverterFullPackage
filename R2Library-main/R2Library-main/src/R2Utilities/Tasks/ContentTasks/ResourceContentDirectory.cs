#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public enum ResourceContentDirectoryType
    {
        Xml,
        Html,
        Images,
        Media
    }

    public class ResourceContentDirectory
    {
        private static readonly string[] ContentTypes = { "xml", "html", "images", "media" };

        public ResourceContentDirectory(ResourceContentDirectoryType contentType, string contentTypeDirectory,
            string isbn)
        {
            ContentType = ContentTypes[(int)contentType];
            ContentTypeDirectory = new DirectoryInfo(contentTypeDirectory);
            var path = Path.Combine(contentTypeDirectory, isbn);
            ResourceDirectory = new DirectoryInfo(path);

            if (!ResourceDirectory.Exists)
            {
                return;
            }

            Files = ResourceDirectory.GetFiles();
            NewestFile = Files.OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();
        }

        public string ContentType { get; }
        public DirectoryInfo ResourceDirectory { get; }
        public DirectoryInfo ContentTypeDirectory { get; private set; }
        public FileInfo[] Files { get; }
        public FileInfo NewestFile { get; }

        public bool IsRestoreRequired(FileInfo backupZipFileInfo)
        {
            if (!ResourceDirectory.Exists)
            {
                return false;
            }

            if (NewestFile == null)
            {
                return false; // directory is empty
            }

            return NewestFile.LastWriteTimeUtc < backupZipFileInfo.LastWriteTimeUtc;
        }

        public bool IsNewestFileNewer(DateTime lastWriteTimeUtc)
        {
            if (NewestFile == null)
            {
                return false; // directory is empty
            }

            return NewestFile.LastWriteTimeUtc > lastWriteTimeUtc || NewestFile.CreationTimeUtc > lastWriteTimeUtc;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder("ResourceContentDirectory = [");
            sb.AppendFormat("ContentType: {0}", ContentType);
            sb.AppendFormat(", ResourceDirectory.FullName: {0}", ResourceDirectory.FullName);
            sb.AppendFormat(", ResourceDirectory.Exists: {0}", ResourceDirectory.Exists);
            sb.AppendFormat(", Files: {0}", Files == null ? 0 : Files.Length);
            sb.AppendFormat(", NewestFile.FullName: {0}", NewestFile == null ? "" : NewestFile.FullName);
            sb.AppendFormat(", NewestFile.LastWriteTimeUtc: {0}",
                NewestFile == null ? "" : $"{NewestFile.LastWriteTimeUtc:u}");
            sb.Append("]");
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}