#region

using System;

#endregion

namespace R2V2.Extensions
{
    public static class TypeExtensions
    {
        public static bool Implements<TInterface>(this Type type)
        {
            return Implements(type, typeof(TInterface));
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            return type.GetInterface(interfaceType.FullName, false) != null;
        }

        public static bool Inherits<TParent>(this Type type)
        {
            return Inherits(type, typeof(TParent));
        }

        public static bool Inherits(this Type type, Type parentType)
        {
            var result = parentType.IsAssignableFrom(type);

            return result;
        }
    }
}