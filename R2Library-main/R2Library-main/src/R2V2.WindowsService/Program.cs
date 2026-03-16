#region

using System;
using System.Linq;
using System.ServiceProcess;

#endregion

namespace R2V2.WindowsService
{
    static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var service = new R2v2WindowsService();

            if (IsDebugMode(args))
            {
                //This allows for easy debugging in Visual Studio
                service.Init();
            }
            else
            {
                //This is what the installed service runs
                ServiceBase[] servicesToRun = { service };
                ServiceBase.Run(servicesToRun);
            }
        }

        private static bool IsDebugMode(string[] args)
        {
            return args.Contains("-debug", StringComparer.CurrentCultureIgnoreCase);
        }
    }
}