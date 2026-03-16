using System;
using System.IO;
using System.Reflection;
using System.Web;
using log4net;

namespace SPExample
{
    public class Global : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        void Application_Start(object sender, EventArgs e)
        {
            string file = HttpRuntime.AppDomainAppVirtualPath + "/log4net.config";
            file = Server.MapPath(file);
            var fi = new FileInfo(file);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fi);
            GlobalContext.Properties["requestId"] = "n/a";

            Log.Info("Application Start >>>>>> ++");
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Log.WarnFormat("Application Error'");
            Exception ex = Server.GetLastError();

            Log.Error(ex.Message, ex);
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            Log.DebugFormat("Session_End() - SessionID: {0}", Session.SessionID);

            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
