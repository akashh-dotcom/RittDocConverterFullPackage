#region

using System;

#endregion

namespace R2V2.Extensions
{
    public static class DateExtensions
    {
        public static bool IsNull(this DateTime instance)
        {
            return instance == default(DateTime);
        }

        public static bool IsNotNull(this DateTime instance)
        {
            return !IsNull(instance);
        }

        public static bool IsNull(this DateTime? instance)
        {
            return !instance.HasValue;
        }

        public static bool IsNotNull(this DateTime? instance)
        {
            return !IsNull(instance);
        }

        public static string ToDebugString(this DateTime? instance)
        {
            return instance == null ? string.Empty : instance.Value.ToString();
        }

        public static string ToDebugString(this DateTime? instance, string format)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            return format != null ? string.Format(format, instance.Value) : instance.Value.ToString();
        }
    }
}