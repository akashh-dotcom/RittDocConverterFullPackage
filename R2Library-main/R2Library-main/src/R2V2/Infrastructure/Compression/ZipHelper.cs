#region

using System;
using System.IO;
using System.Reflection;
using Common.Logging;
using Ionic.Zip;
using Ionic.Zlib;

#endregion

namespace R2V2.Infrastructure.Compression
{
    public class ZipHelper
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool CompressFile(string fileToCompress, string zipFileName)
        {
            Log.InfoFormat("CompressFile() - zipFileName: {0}", zipFileName);
            try
            {
                using (var zip = new ZipFile(zipFileName))
                {
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddFile(fileToCompress, "");
                    zip.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
        }

        public static bool CompressFile(string fileToCompress, string zipFileName, string directoryName)
        {
            Log.InfoFormat("CompressFile() - zipFileName: {0}", zipFileName);
            try
            {
                using (var zip = new ZipFile(zipFileName))
                {
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddFile(fileToCompress, directoryName);
                    zip.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
        }

        public static bool CompressFiles(string[] filesToCompress, string zipFileName)
        {
            Log.InfoFormat("CompressFiles() - zipFileName: {0}", zipFileName);
            try
            {
                using (var zip = new ZipFile(zipFileName))
                {
                    zip.CompressionLevel = CompressionLevel.BestCompression;

                    foreach (var fileToCompress in filesToCompress)
                    {
                        zip.CompressionLevel = CompressionLevel.BestCompression;
                        zip.AddFile(fileToCompress, "");
                        zip.Save();
                    }

                    zip.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
        }

        public static void CompressDirectory(string directory)
        {
            Log.InfoFormat("CompressDirectory() - directory: {0}", directory);
            try
            {
                var zipFileName =
                    $"{(directory.EndsWith(@"\") ? directory.Substring(0, directory.Length - 1) : directory)}.zip";
                using (var zip = new ZipFile(zipFileName))
                {
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddDirectory(directory);
                    zip.Save();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public static void CompressDirectory(string directory, string zipFileName)
        {
            Log.InfoFormat("CompressDirectory() - directory: {0}, zipFileName: {1}", directory, zipFileName);
            try
            {
                using (var zip = new ZipFile(zipFileName))
                {
                    zip.CompressionLevel = CompressionLevel.BestCompression;

                    //Clear out any existing entries, in the event that this zip file already exists
                    zip.RemoveSelectedEntries("*");

                    zip.AddDirectory(directory);
                    zip.Save();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public static void CompressDirectory(string directory, string zipFileName, string directoryName)
        {
            Log.InfoFormat("CompressDirectory() - zipFileName: {0}, zipFileName: {1}, directoryName: {2}", zipFileName,
                zipFileName, directoryName);
            try
            {
                using (var zip = new ZipFile(zipFileName))
                {
                    zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddDirectory(directory, directoryName);
                    zip.Save();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public static void ExtractAll(string zipFileName, string outputDirectory)
        {
            ExtractAll(zipFileName, outputDirectory, false);
        }

        public static void ExtractAll(string zipFileName, string outputDirectory, bool overwrite)
        {
            var zip = ZipFile.Read(zipFileName);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            zip.ExtractAll(outputDirectory,
                overwrite ? ExtractExistingFileAction.OverwriteSilently : ExtractExistingFileAction.Throw);
        }
    }
}