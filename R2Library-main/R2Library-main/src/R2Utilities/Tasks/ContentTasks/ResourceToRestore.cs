#region

using System.IO;
using System.Text;
using Newtonsoft.Json;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class ResourceToRestore
    {
        public ResourceToRestore(IResource resource, IContentSettings contentSettings,
            IR2UtilitiesSettings r2UtilitiesSettings)
        {
            Isbn = resource.Isbn;

            Xml = new ResourceContentDirectory(ResourceContentDirectoryType.Xml, contentSettings.ContentLocation, Isbn);
            Html = new ResourceContentDirectory(ResourceContentDirectoryType.Html,
                Path.Combine(contentSettings.NewContentLocation, "html"), Isbn);
            Images = new ResourceContentDirectory(ResourceContentDirectoryType.Images,
                contentSettings.ImageBaseFileLocation, Isbn);
            Media = new ResourceContentDirectory(ResourceContentDirectoryType.Media,
                Path.Combine(contentSettings.NewContentLocation, "media"), Isbn);

            var bookCoverImageRootPath = Path.Combine(contentSettings.ImageBaseFileLocation, "book-covers");
            BookCoverDirectory = new DirectoryInfo(bookCoverImageRootPath);
            //string bookCoverImagePath = Path.Combine(bookCoverImageRootPath, string.Format("{0}.jpg", Isbn));
            //BookCoverImage = new FileInfo(bookCoverImagePath);

            var zipFilePath = Path.Combine(r2UtilitiesSettings.ContentBackupDirectory, $"{Isbn}.zip");
            BackupZipFile = new FileInfo(zipFilePath);

            WorkingDirectory =
                new DirectoryInfo(Path.Combine(r2UtilitiesSettings.ContentBackupDirectory, "_working", Isbn));
            XmlWorkingDirectory = new DirectoryInfo(Path.Combine(WorkingDirectory.FullName, "xml"));
            HtmlWorkingDirectory = new DirectoryInfo(Path.Combine(WorkingDirectory.FullName, "html"));
            ImagesWorkingDirectory = new DirectoryInfo(Path.Combine(WorkingDirectory.FullName, "images"));
            MediaWorkingDirectory = new DirectoryInfo(Path.Combine(WorkingDirectory.FullName, "media"));
            BookCoverImageWorkingDirectory = new DirectoryInfo(Path.Combine(WorkingDirectory.FullName, "book-covers"));
            //BookCoverImageWorking = new FileInfo(Path.Combine(WorkingDirectory.FullName, "book-covers", string.Format("{0}.jpg", Isbn)));

            if (!BackupZipFile.Exists)
            {
                ResourceRestoreRequired = false;
                return;
            }

            if (!Xml.ResourceDirectory.Exists && !Html.ResourceDirectory.Exists && !Images.ResourceDirectory.Exists &&
                !Media.ResourceDirectory.Exists)
            {
                ResourceRestoreRequired = true;
                return;
            }

            ResourceRestoreRequired = Xml.IsRestoreRequired(BackupZipFile) ||
                                      Html.IsRestoreRequired(BackupZipFile) ||
                                      Images.IsRestoreRequired(BackupZipFile) ||
                                      Media.IsRestoreRequired(BackupZipFile); // ||
            //((!BookCoverImage.Exists) || (BookCoverImage.LastAccessTimeUtc < BackupZipFile.LastAccessTimeUtc))
            //);
        }

        public string Isbn { get; }

        public ResourceContentDirectory Xml { get; }
        public ResourceContentDirectory Html { get; }
        public ResourceContentDirectory Images { get; }
        public ResourceContentDirectory Media { get; }

        //public FileInfo BookCoverImage { get; private set; }
        public FileInfo BackupZipFile { get; }
        public bool ResourceRestoreRequired { get; }

        public DirectoryInfo WorkingDirectory { get; }
        public DirectoryInfo XmlWorkingDirectory { get; private set; }
        public DirectoryInfo HtmlWorkingDirectory { get; private set; }
        public DirectoryInfo ImagesWorkingDirectory { get; private set; }
        public DirectoryInfo MediaWorkingDirectory { get; private set; }
        public DirectoryInfo BookCoverImageWorkingDirectory { get; private set; }

        public DirectoryInfo BookCoverDirectory { get; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("ResourcePaths = [");
            sb.AppendFormat("Isbn: {0}", Isbn);
            sb.AppendFormat(", ResourceRestoreRequired: {0}", ResourceRestoreRequired);

            sb.AppendLine().Append("\t");
            sb.AppendFormat(", Xml: {0}", Xml == null ? "null" : Xml.ToDebugString());
            sb.AppendLine().Append("\t");
            sb.AppendFormat(", Html: {0}", Html == null ? "null" : Html.ToDebugString());
            sb.AppendLine().Append("\t");
            sb.AppendFormat(", Images: {0}", Images == null ? "null" : Images.ToDebugString());
            sb.AppendLine().Append("\t");
            sb.AppendFormat(", Media: {0}", Media == null ? "null" : Media.ToDebugString());

            sb.AppendLine().Append("\t");
            //sb.AppendFormat(", BookCoverImage.FullName: {0}", (BookCoverImage == null) ? "" : BookCoverImage.FullName);
            //sb.AppendFormat(", BookCoverImage.LastWriteTimeUtc: {0}", (BookCoverImage == null) ? "" : string.Format("{0:u}", BookCoverImage.LastWriteTimeUtc));
            sb.AppendFormat(", BookCoverDirectory.Exists: {0}", BookCoverDirectory.Exists);
            sb.AppendFormat(", BookCoverDirectory.FullName: {0}", BookCoverDirectory.FullName);
            sb.AppendLine().Append("\t");
            sb.AppendFormat(", BackupZipFile.FullName: {0}", BackupZipFile == null ? "" : BackupZipFile.FullName);
            sb.AppendFormat(", BackupZipFile.LastWriteTimeUtc: {0}",
                BackupZipFile == null ? "" : $"{BackupZipFile.LastWriteTimeUtc:u}");
            sb.AppendLine().Append("\t");
            sb.Append("]");
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}