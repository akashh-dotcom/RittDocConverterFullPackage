#region

using System.Configuration;
using System.IO;

#endregion

namespace R2V2.XslTest
{
    internal static class Shell
    {
        private static readonly string FilePath = ConfigurationManager.AppSettings["App.ShellPath"];

        internal static string Wrap(string html)
        {
            return string.Format(
                File.ReadAllText(FilePath),
                html
            );
        }
    }
}