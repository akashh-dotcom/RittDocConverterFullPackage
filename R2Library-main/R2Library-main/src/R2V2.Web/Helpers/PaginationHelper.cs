#region

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Helpers
{
    public class PaginationHelper
    {
        const string AllKey = "All";
        const string RecentKey = "Recent";
        const string NumericKey = "#";
        const string NumericKeyValue = "09";

        public static IEnumerable<PageLink> GetAlphaPageLinks(IEnumerable<string> keys, string selectedKey,
            bool showAll, bool showRecent)
        {
            var alphaPageLinks = GetAlphaPageLinks(keys, showAll, showRecent).ToList();

            if (!string.IsNullOrWhiteSpace(selectedKey))
            {
                foreach (var alphaPageLink in alphaPageLinks)
                {
                    alphaPageLink.Selected = alphaPageLink.Text == selectedKey ||
                                             (alphaPageLink.Text == NumericKey && selectedKey == NumericKeyValue);
                }
            }

            return alphaPageLinks;
        }

        public static IEnumerable<PageLink> GetAlphaPageLinks(IEnumerable<string> keys, string selectedKey,
            bool showAll)
        {
            var alphaPageLinks = GetAlphaPageLinks(keys, showAll).ToList();

            if (!string.IsNullOrWhiteSpace(selectedKey))
            {
                foreach (var alphaPageLink in alphaPageLinks)
                {
                    alphaPageLink.Selected = alphaPageLink.Text == selectedKey ||
                                             (alphaPageLink.Text == NumericKey && selectedKey == NumericKeyValue);
                }
            }

            return alphaPageLinks;
        }

        public static IEnumerable<PageLink> GetAlphaPageLinks(IEnumerable<string> keys, bool showAll,
            bool showRecent = false)
        {
            keys = keys.Select(key => key.ToUpper()).ToList();

            var alphaKeys = showAll
                ? new[]
                {
                    "All", "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q",
                    "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
                }
                : new[]
                {
                    "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S",
                    "T", "U", "V", "W", "X", "Y", "Z"
                };

            if (showRecent)
            {
                alphaKeys = showAll
                    ? new[]
                    {
                        "Recent", "All", "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
                        "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
                    }
                    : new[]
                    {
                        "Recent", "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
                        "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
                    };
            }

            return alphaKeys.Select(alphaKey => BuildPageLink(keys, alphaKey));
        }

        private static PageLink BuildPageLink(IEnumerable<string> alphaKeys, string alphaKey)
        {
            var keys = alphaKeys.ToList();
            var key = alphaKey == NumericKey ? NumericKeyValue : alphaKey;

            var regex = new Regex("[0-9]");

            return new PageLink
            {
                Text = alphaKey,
                Href = $"#{key}",
                Active = keys.Contains(alphaKey) || alphaKey == NumericKey && keys.Any(regex.IsMatch) ||
                         alphaKey == AllKey || alphaKey == RecentKey
            };
        }
    }
}