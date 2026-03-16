#region

using System.Configuration;

#endregion

namespace R2V2.XslTest
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            Bootstrapper.Initialize(ConfigurationManager.AppSettings["SettingsConfigurationKey"]);

            //Transformer.WriteAllText(path, AutoDatabaseSettings.ConfigurationSettings["App"]["BaseUrl"]);
        }
    }
}