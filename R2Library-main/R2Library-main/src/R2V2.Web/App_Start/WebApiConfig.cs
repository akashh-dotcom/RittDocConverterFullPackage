#region

using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using log4net;

#endregion

namespace R2V2.Web
{
    public static class WebApiConfig
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            // Configure XML formatter
            var xmlFormatter = config.Formatters.XmlFormatter;
            xmlFormatter.UseXmlSerializer = true; // Use XmlSerializer for precise control over XML structure
            xmlFormatter.Indent = true; // Optional: Makes XML output more readable
            xmlFormatter.WriterSettings.OmitXmlDeclaration = false; // Ensure XML declaration is included
            xmlFormatter.SupportedMediaTypes.Clear(); // Clear defaults to avoid conflicts
            xmlFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));

            Log.Debug("WebApiConfig Register Complete");
        }
    }
}