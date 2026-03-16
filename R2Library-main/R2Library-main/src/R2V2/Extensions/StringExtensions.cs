#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace R2V2.Extensions
{
    public static class StringExtensions
    {
        public static void AppendFormatedLine(this StringBuilder builder, string message, params object[] args)
        {
            builder.AppendFormat(message, args);
            builder.AppendLine();
        }

        public static bool Contains(this string value, string matchOn, StringComparison comparison)
        {
            return value.IndexOf(matchOn, comparison) >= 0;
        }

        public static string StripHtml(this string value)
        {
            return ReplaceWithRegExp(value, "</?[A-z]+>", "");
        }

        public static string Remove(this string root, string termToRemove)
        {
            if (root.IsEmpty())
            {
                return root;
            }

            return root.Replace(termToRemove, string.Empty);
        }

        public static string MakeReadable(this string root)
        {
            return root.ReplaceWithRegExp("([A-Z])", " $1");
        }

        public static string RemoveNonAsciiCharacters(this string root)
        {
            return root.ReplaceWithRegExp(@"[^\u0000-\u007F]", "");
        }

        public static string ReplaceWithRegExp(this string root, string pattern, string expression)
        {
            return root.IsEmpty() ? root : Regex.Replace(root, pattern, expression);
        }

        public static string RemoveWithRegExp(this string root, string pattern)
        {
            return ReplaceWithRegExp(root, pattern, string.Empty);
        }

        public static bool IsEmpty(this string root)
        {
            return string.IsNullOrEmpty(root);
        }

        public static string BuildUrl(this string value, string path)
        {
            return value.IsNotEmpty() ? Path.Combine(path, value.Replace("/", "")) : null;
        }

        public static string RemoveHtml(this string root)
        {
            if (root.IsEmpty())
            {
                return root;
            }

            return root.Replace("&amp;", "&").Replace("&trade;", "©").RemoveWithRegExp("<(.)*?>")
                .RemoveWithRegExp("^(.)*?>");
        }

        public static string Linkify(this string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // www|http|https|ftp|news|file
                text = Regex.Replace(
                        text,
                        @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])",
                        "<a href=\"$1\" target=\"_blank\">$1</a>",
                        RegexOptions.IgnoreCase)
                    .Replace("href=\"www", "href=\"http://www");

                // mailto
                text = Regex.Replace(
                    text,
                    @"(([a-zA-Z0-9_\-\.])+@[a-zA-Z\ ]+?(\.[a-zA-Z]{2,6})+)",
                    "<a href=\"mailto:$1\">$1</a>",
                    RegexOptions.IgnoreCase);
            }

            return text;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (value == null)
            {
                return null;
            }

            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        public static bool IsNumeric(this string root)
        {
            return root.Matches("^[0-9]+$");
        }

        public static string ExtractBetween(this string root, string matchLeft, params string[] matchRight)
        {
            var indexLeft = root.LastIndexOf(matchLeft);
            var offsetLeft = indexLeft + matchLeft.Length;

            if (indexLeft < 0 && offsetLeft <= root.Length)
            {
                return string.Empty;
            }

            foreach (var matchRightOn in matchRight)
            {
                var indexRight = root.IndexOf(matchRightOn, offsetLeft);

                if (indexRight > 0)
                {
                    return root.Substring(offsetLeft, indexRight - offsetLeft);
                }
            }

            return string.Empty;
        }

        public static bool ContainsRegExp(this string root, string match)
        {
            return root.Matches(match);
        }

        public static string ExtractFromLastMatchToEnd(this string root, string match)
        {
            var index = root.LastIndexOf(match);
            var offset = index + match.Length;

            if (index < 0 && offset <= root.Length)
            {
                return string.Empty;
            }

            return root.Substring(offset);
        }

        public static StringOptions Args(this string message, params object[] args)
        {
            return new StringOptions(string.Format(message, args));
        }

        public static string ParseString(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            value = Regex.Replace(value, @"\s*([=*/+-,&!@#%^])\s*", "_");
            return value.Replace(" ", "_");
        }

        public static StringOptions Add(this string root, string value, string seperater)
        {
            return new StringOptions(root).Add(value, seperater);
        }

        public static StringOptions WrapIf(this string root, bool test, string prefix, string postfix)
        {
            return test ? Wrap(root, prefix, postfix) : new StringOptions(root);
        }

        public static bool Matches(this string root, string pattern)
        {
            if (root.IsEmpty())
            {
                return false;
            }

            var expression = Regex.Match(root, pattern);
            return expression.Success;
        }

        public static StringOptions Append(this string root, StringCollection dataToAppend)
        {
            var option = new StringOptions(root);

            foreach (var append in dataToAppend)
            {
                option.Append(append);
            }

            return option;
        }

        public static StringBuilder ToBuilder(this string root)
        {
            return new StringBuilder(root);
        }

        public static StringOptions Append(this string root, string dataToAppend)
        {
            return new StringOptions(root).Append(dataToAppend);
        }

        public static StringOptions Append(this string root, string formatedDataToAppend, params object[] args)
        {
            return new StringOptions(root).Append(formatedDataToAppend, args);
        }

        public static StringOptions ToOptions(this string root)
        {
            return new StringOptions(root);
        }

        public static StringOptions AppendLine(this string root)
        {
            return new StringOptions(root).AppendLine();
        }

        public static StringOptions AppendLine(this string root, string dataToAppend)
        {
            return new StringOptions(root).AppendLine(dataToAppend);
        }

        public static StringOptions AppendLine(this string root, string formatedDataToAppend, params object[] args)
        {
            return new StringOptions(root).AppendLine(formatedDataToAppend, args);
        }

        public static StringOptions Prefix(this string root, string prefix)
        {
            return new StringOptions(root).Prefix(prefix);
        }

        public static StringOptions Wrap(this string root, string prefix, string postfix)
        {
            return new StringOptions(root).Wrap(prefix, postfix);
        }

        public static bool IsNotEmpty(this string root)
        {
            return !string.IsNullOrEmpty(root);
        }

        public static StringOptions NewLine(this string root)
        {
            return new StringOptions(root).NewLine();
        }

        public static string ReplaceMany(this string value, string newValue, params string[] oldValues)
        {
            return oldValues.Aggregate(value, (current, oldValue) => current.Replace(oldValue, newValue));
        }

        public static IEnumerable<string> GetDelimitedItems(this string value, params string[] delimiters)
        {
            return value.ReplaceMany(",", delimiters).Split(',');
        }

        public static T? TryParse<T>(this string value)
            where T : struct
        {
            if (typeof(T) == typeof(int))
            {
                return
                    int.TryParse(value, out var result)
                        ? result as T?
                        : null;
            }

            throw new NotImplementedException("TryParse<{0}> not implemented".Args(typeof(T)));
        }

        public static T? TryParseExact<T>(this string value, string dateFormat)
            where T : struct
        {
            if (typeof(T) == typeof(DateTime))
            {
                return
                    DateTime.TryParseExact(value, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out var result)
                        ? result as T?
                        : null;
            }

            throw new NotImplementedException("TryParseExact<{0}> not implemented".Args(typeof(T)));
        }


        #region Nested type: StringOptions

        public class StringOptions
        {
            private readonly StringBuilder _builder;

            public StringOptions()
            {
                _builder = new StringBuilder();
            }

            public StringOptions(string initalialValue)
            {
                _builder = new StringBuilder(initalialValue);
            }

            public StringOptions Append(string dataToAppend)
            {
                _builder.Append(dataToAppend);

                return this;
            }

            public StringOptions WrapIf(bool test, string prefix, string postfix)
            {
                return test ? Wrap(prefix, postfix) : this;
            }

            public StringOptions Append(string formatedDataToAppend, params object[] args)
            {
                _builder.AppendFormat(formatedDataToAppend, args);

                return this;
            }

            public StringOptions AppendLine()
            {
                _builder.AppendLine();

                return this;
            }

            public StringOptions AppendLine(string dataToAppend)
            {
                _builder.AppendLine(dataToAppend);

                return this;
            }

            public StringOptions AppendLine(string formatedDataToAppend, params object[] args)
            {
                _builder.AppendFormat(formatedDataToAppend, args).AppendLine();

                return this;
            }


            public StringOptions NewLine()
            {
                _builder.AppendLine();

                return this;
            }

            public StringOptions Prefix(string prefix)
            {
                if (prefix.IsNotEmpty())
                {
                    _builder.Insert(0, prefix);
                }

                return this;
            }

            public override string ToString()
            {
                return _builder.ToString();
            }

            public static implicit operator string(StringOptions options)
            {
                return options.ToString();
            }

            public StringOptions Wrap(string prefix, string postfix)
            {
                Prefix(prefix);
                Append(postfix);

                return this;
            }

            public StringOptions Add(string value, string seperater)
            {
                if (_builder.Length == 0)
                {
                    Append(value);
                }
                else
                {
                    Append("{0}{1}", seperater, value);
                }

                return this;
            }
        }

        #endregion
    }
}