#region

using System.Collections.Generic;
using System.Text.RegularExpressions;
using R2V2.Extensions;

#endregion

namespace R2V2.Core.R2Utilities
{
    public static class IsbnUtilities
    {
        public static IEnumerable<string> GetDelimitedIsbns(string isbns)
        {
            return isbns
                .Trim()
                .Replace("-", "")
                .GetDelimitedItems(",", "/", "\\", " ", "\r", "\n");
        }

        /// <summary>
        /// Logic that will validate the isbn, making sure that it conforms to the internationally defined standard format of an ISBN.  Use RegEx for this.
        /// http://www.regexlib.com/(A(bX0gu7eaQW4XW0EGUdb7rjoZUMePQd8wxI8H-I3GMYIs8QUzwX0HClMdYohnr-kBTOSggRsTTpJk30y5LOe83sUZJIPvVsoPAWZkyKYJsrgAVIRnlObcaTbleCHV7ACZneurNzosxRQ-_eVhefimhZMt4grc47-RlE79dkWjgGEcT6ZrEF2C2cVP-m4Ugbaj0))/Search.aspx?k=isbn&c=-1&m=-1&ps=20
        //  Expression = ^(97(8|9))?\d{9}(\d|X)$
        //  How to use REGEX with C# - http://www.c-sharpcorner.com/UploadFile/prasad_1/RegExpPSD12062005021717AM/RegExpPSD.aspx
        /// </summary>
        /// <returns>bool</returns>
        public static bool IsValidIsbn(string isbn)
        {
            //what's the @ sign for?  to escape the \ character.  http://stackoverflow.com/questions/1558058/bad-compile-constant-value
            var isbnRegex = new Regex(@"^(97(8|9))?\d{9}(\d|X)$");
            return isbnRegex.IsMatch(isbn);
        }
    }
}