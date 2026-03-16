#region

using System;

#endregion

namespace R2V2.Infrastructure.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotRegisterWithContainerAttribute : Attribute
    {
    }
}