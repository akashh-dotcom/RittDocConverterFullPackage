#region

using System;

#endregion

namespace R2V2.Extensions
{
    public static class ObjectExtensions
    {
        public static T As<T>(this object o)
        {
            if (o is T o1)
            {
                return o1;
            }

            throw new Exception("Unable to cast {0} to {1}".Args(o.GetType(), typeof(T)));
        }
    }
}