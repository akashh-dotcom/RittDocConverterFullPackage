#region

using System.Web.Mvc;

#endregion

namespace R2V2.Web.Infrastructure.Contexts
{
    public class ActionContext
    {
        public ActionContext(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            ControllerContext = controllerContext;
            ActionDescriptor = actionDescriptor;
        }

        public ControllerContext ControllerContext { get; private set; }
        public ActionDescriptor ActionDescriptor { get; private set; }
    }
}