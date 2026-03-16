#region

using System;

#endregion

namespace R2V2.Extensions
{
    public static class ReferenceExtensions
    {
        public static TOut IfNotNull<TIn, TOut>(this TIn v, Func<TIn, TOut> f)
            where TIn : class
            where TOut : class
        {
            return v == null ? null : f(v);
        }

        public static bool IfNotNull<TIn>(this TIn v, Func<TIn, bool> f)
            where TIn : class
        {
            return v != null && f(v);
        }
    }
}