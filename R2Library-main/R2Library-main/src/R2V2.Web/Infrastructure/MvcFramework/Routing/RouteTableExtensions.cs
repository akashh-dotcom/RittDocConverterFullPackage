#region

using System;
using System.Web.Routing;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Routing
{
    public static class RouteTableExtensions
    {
        public static Route MapR2V2Route(this RouteCollection routes, string name, string url)
        {
            return MapR2V2Route(routes, name, url, null /* defaults */, (object)null /* constraints */);
        }

        public static Route MapR2V2Route(this RouteCollection routes, string name, string url, object defaults)
        {
            return MapR2V2Route(routes, name, url, defaults, (object)null /* constraints */);
        }

        public static Route MapR2V2Route(this RouteCollection routes, string name, string url, object defaults,
            object constraints)
        {
            return MapR2V2Route(routes, name, url, defaults, constraints, null /* namespaces */);
        }

        public static Route MapR2V2Route(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return MapR2V2Route(routes, name, url, null /* defaults */, null /* constraints */, namespaces);
        }

        public static Route MapR2V2Route(this RouteCollection routes, string name, string url, object defaults,
            string[] namespaces)
        {
            return MapR2V2Route(routes, name, url, defaults, null /* constraints */, namespaces);
        }

        public static Route MapR2V2Route(this RouteCollection routes, string name, string url, object defaults,
            object constraints, string[] namespaces)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            var route = new Route(url, new R2V2RouteHandler())
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            };

            if (namespaces != null && namespaces.Length > 0)
            {
                route.DataTokens["Namespaces"] = namespaces;
            }

            routes.Add(name, route);

            return route;
        }
    }
}