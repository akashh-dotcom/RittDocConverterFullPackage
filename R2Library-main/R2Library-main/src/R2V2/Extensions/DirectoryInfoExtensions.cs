#region

using System.IO;
using System.Linq;

#endregion

namespace R2V2.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static FileInfo GetNewestFile(this DirectoryInfo directoryInfo)
        {
            return directoryInfo.GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();
        }

        public static void Empty(this DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                return;
            }

            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }
    }

    public static class DirectoryHelper
    {
        public static void VerifyDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}