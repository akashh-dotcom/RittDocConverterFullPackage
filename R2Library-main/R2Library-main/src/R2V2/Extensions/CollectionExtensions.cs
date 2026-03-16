#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#endregion

namespace R2V2.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.IsNotEmpty();
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any();
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection.IsNotEmpty())
            {
                foreach (var item in collection)
                    action(item);
            }
        }

        public static string AsString<T>(this IEnumerable<T> collection, Func<T, string> adapter, string seperator)
        {
            if (collection == null)
            {
                return "";
            }

            var builder = new StringBuilder();

            foreach (var s in collection)
            {
                if (builder.Length > 0)
                    builder.Append(seperator);

                builder.Append(adapter(s));
            }

            return builder.ToString();
        }

        public static string ToDelimitedList(this IEnumerable<string> list)
        {
            return ToDelimitedList(list, ",");
        }

        public static string ToDelimitedList(this IEnumerable<string> list, string delimiter)
        {
            return string.Join(delimiter, list);
        }

        [DebuggerStepThrough]
        public static void Each<T>(this IEnumerable<T> collection, Action<T> command)
        {
            if (collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                command(item);
            }
        }

        public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
        {
            var toReturn = new List<T>(max);
            foreach (var item in source)
            {
                toReturn.Add(item);
                if (toReturn.Count != max) continue;
                yield return toReturn;
                toReturn = new List<T>(max);
            }

            if (toReturn.Any())
            {
                yield return toReturn;
            }
        }
    }
}