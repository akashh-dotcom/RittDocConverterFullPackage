#region

using System.Web.Mvc;
using R2V2.Web.Infrastructure.Contexts;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public interface IR2V2Filter : IMvcFilter
    {
        FilterScope FilterScope { get; }
        bool CanProcess(ActionContext actionContext);
    }
}