#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using R2V2.Extensions;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;

#endregion

namespace R2V2.Web.Helpers
{
    public static class UrlHelperExtensions
    {
        private const string StaticFolderUrl = "~/_Static";
        private static readonly IDictionary<string, string> StaticSignedPathOverMap = LoadHashes();

        private static IDictionary<string, string> LoadHashes()
        {
            var hashes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var datFilePath = HttpRuntime.AppDomainAppPath + "_static.dat";

            if (File.Exists(datFilePath))
            {
                using (var sr = new StreamReader(datFilePath))
                {
                    while (sr.Peek() >= 0)
                    {
                        var readLine = sr.ReadLine();
                        if (readLine == null)
                        {
                            continue;
                        }

                        var parts = readLine.Split('|');
                        hashes.Add(parts[0], parts[1]);
                    }
                }
            }

            return hashes;
        }

        private static string Static(string path)
        {
            if (StaticSignedPathOverMap.ContainsKey(path))
                path = StaticSignedPathOverMap[path];

            return VirtualPathUtility.ToAbsolute("{0}/{1}".Args(StaticFolderUrl, path));
        }

        public static string Image(this UrlHelper urlHelper, string fileName)
        {
            return Static("Images/{0}".Args(fileName));
        }

        public static string Script(this UrlHelper urlHelper, string fileName)
        {
            return Static("Scripts/{0}".Args(fileName));
        }

        // Helper to extract route values from an expression
        private static RouteValueDictionary GetRouteValuesFromExpression<TController>(
            Expression<Action<TController>> action)
            where TController : Controller
        {
            var methodCall = action.Body as MethodCallExpression;
            if (methodCall == null)
                throw new ArgumentException("Action must be a method call", nameof(action));

            var routeValues = new RouteValueDictionary
            {
                { "controller", typeof(TController).Name.Replace("Controller", "") },
                { "action", methodCall.Method.Name }
            };

            var parameters = methodCall.Method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var argument = methodCall.Arguments[i];
                object value = null;
                if (argument is ConstantExpression constExpr)
                {
                    value = constExpr.Value;
                }
                else
                {
                    // Compile and evaluate the expression
                    value = Expression.Lambda(argument).Compile().DynamicInvoke();
                }
                routeValues.Add(parameters[i].Name, value);
            }

            return routeValues;
        }

        public static string Action<TController>(this UrlHelper urlHelper, Expression<Action<TController>> action)
            where TController : Controller
        {
            var valuesFromExpression = GetRouteValuesFromExpression(action);
            valuesFromExpression.Add("Area", "");

            var virtualPathForArea =
                urlHelper.RouteCollection.GetVirtualPathForArea(urlHelper.RequestContext, valuesFromExpression);
            return virtualPathForArea != null ? virtualPathForArea.VirtualPath : null;
        }

        public static string AdminAction<TController>(this UrlHelper urlHelper, Expression<Action<TController>> action)
            where TController : R2AdminBaseController
        {
            var valuesFromExpression = GetRouteValuesFromExpression(action);
            valuesFromExpression.Add("Area", "Admin");

            var virtualPathForArea =
                urlHelper.RouteCollection.GetVirtualPathForArea(urlHelper.RequestContext, valuesFromExpression);
            return virtualPathForArea != null ? virtualPathForArea.VirtualPath : null;
        }
    }
}