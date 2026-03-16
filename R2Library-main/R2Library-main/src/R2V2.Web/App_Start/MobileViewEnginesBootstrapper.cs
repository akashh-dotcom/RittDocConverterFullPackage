#region

using R2V2.Web.Infrastructure.MvcFramework;
using System.Web.Mvc;
using WebActivatorEx;

#endregion

[assembly: PreApplicationStartMethod(typeof(R2V2.Web.MobileViewEngines), "Start")]

namespace R2V2.Web
{
    public static class MobileViewEngines
    {
        public static void Start()
        {
            ViewEngines.Engines.Insert(0, new MobileCapableRazorViewEngine());
        }
    }
}