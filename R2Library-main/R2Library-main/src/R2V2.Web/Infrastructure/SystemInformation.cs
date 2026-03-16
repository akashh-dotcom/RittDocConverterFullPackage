#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using R2V2.Extensions;

#endregion

namespace R2V2.Web.Infrastructure
{
    public static class SystemInformation
    {
        public static readonly bool IsInDebugMode;

        static SystemInformation()
        {
            IsInDebugMode = Debugger.IsAttached;
        }

        public static string ExecutionPath => HttpContext.Current != null
            ? HttpContext.Current.Server.MapPath("~")
            : AppDomain.CurrentDomain.BaseDirectory;

        public static string OutputDirectory
        {
            get
            {
                var outputPath = Path.Combine(ExecutionPath, "Output");

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                return outputPath;
            }
        }

        public static string ConfigurationFileDirectory => ExecutionPath;

        public static string HibernateConfigurationFile => CalculateConfigurationFilePath("hibernate.config");

        public static string HbmOutputDirectory
        {
            get
            {
                var hbmPath = Path.Combine(OutputDirectory, @"hbm\");

                if (!Directory.Exists(hbmPath))
                {
                    Directory.CreateDirectory(hbmPath);
                }

                return hbmPath;
            }
        }

        public static IEnumerable<Assembly> ApplicationAssemblies
        {
            get
            {
                return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith(DefaultNamespace))
                    .ToList();
            }
        }

        public static string DefaultNamespace => "R2V2";

        public static string CoreNamespace => DefaultNamespace.Append(".Core");

        public static string ApplicationName => "Rittenhouse";

        public static string OutgoingMessagingEmail
        {
            get => "no-reply@docucare.wkhpe.com";
            set => throw new NotImplementedException();
        }


        public static string CalculateConfigurationFilePath(string configurationFile)
        {
            return Path.Combine(ConfigurationFileDirectory, configurationFile);
        }

        public static bool IsOurType(Type modelType)
        {
            return modelType.Namespace != null && modelType.Namespace.StartsWith(DefaultNamespace);
        }
    }
}