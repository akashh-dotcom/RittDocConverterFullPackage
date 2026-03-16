#region

using System;
using System.IO;
using System.Reflection;
using Common.Logging;
using ICSharpCode.SharpZipLib.Zip;

#endregion

namespace R2Utilities.Utilities
{
    public static class FileCompression
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public static void CompressDirectory(string directory)
        {
            try
            {
                var zipFileName =
                    $"{(directory.EndsWith(@"\") ? directory.Substring(0, directory.Length - 1) : directory)}.zip";
                var filenames = Directory.GetFiles(directory);
                // Zip up the files - From SharpZipLib Demo Code))
                using (var s = new ZipOutputStream(File.Create(zipFileName)))
                {
                    s.SetLevel(9); // 0-9, 9 being the highest compression
                    var buffer = new byte[4096];
                    foreach (var file in filenames)
                    {
                        var entry = new ZipEntry(Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (var fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }

                    s.Finish();
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}