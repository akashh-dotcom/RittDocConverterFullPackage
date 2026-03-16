#region

using System;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;

#endregion

namespace R2V2.Extensions
{
    public static class RegistrationExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            DefaultRegistration(this
                IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
                registration)
        {
            if (registration == null) throw new ArgumentNullException("registration");


            return registration.AsImplementedInterfaces().AsSelf();
        }
    }
}