#region

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Common.Logging;

#endregion

namespace R2Utilities.Utilities
{
    public class IsbnUtilities
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public static string ConvertIsbn(string isbn)
        {
            if (!IsValidateIsbn(isbn))
            {
                throw new IsbnException($"Invalid ISBN = '{isbn}'", "InvalidIsbn");
            }

            var cleanedIsbn = CleanIsbn(isbn);

            if (cleanedIsbn.Length == 10)
            {
                return ConvertIsbn10ToIsbn13(isbn);
            }

            if (cleanedIsbn.Length == 13)
            {
                return ConvertIsbn13ToIsbn10(isbn);
            }

            Log.WarnFormat("Invalid ISBN: '{0}'", isbn);
            throw new IsbnException($"Invalid ISBN length, ISBN = '{isbn}'", "InvalidIsbnLength");
        }


        public static string ConvertIsbn10ToIsbn13(string isbn10)
        {
            if (!IsValidateIsbn(isbn10))
            {
                throw new IsbnException($"Invalid ISBN = '{isbn10}'", "InvalidIsbn");
            }

            var cleanedIsbn10 = CleanIsbn(isbn10);

            if (cleanedIsbn10.Length == 10)
            {
                var result = 0;
                var isbn13 = "978" + isbn10.Substring(0, 9);
                for (var i = 0; i < isbn13.Length; i++)
                {
                    result += int.Parse(isbn13[i].ToString()) * (i % 2 == 0 ? 1 : 3);
                }

                var checkDigit = 10 - result % 10;
                isbn13 = $"{isbn13}{checkDigit.ToString()}";
                Log.DebugFormat("converted {0} --> {1}, new ISBN 13 is valid? {2}", isbn10, isbn13,
                    IsValidateIsbn(isbn13));
                return isbn13;
            }

            Log.WarnFormat("Invalid ISBN 10: '{0}'", isbn10);
            throw new IsbnException($"Invalid ISBN 10 length, ISBN = '{isbn10}'", "InvalidIsbn10Length");
        }

        public static string ConvertIsbn13ToIsbn10(string isbn13)
        {
            if (!IsValidateIsbn(isbn13))
            {
                throw new IsbnException($"Invalid ISBN = '{isbn13}'", "InvalidIsbn");
            }

            var cleanedIsbn13 = CleanIsbn(isbn13);

            if (cleanedIsbn13.Length == 13)
            {
                var total = 0;
                for (var x = 0; x < 11; x++)
                {
                    var factor = x % 2 == 0 ? 1 : 3;
                    total += Convert.ToInt32(cleanedIsbn13[x]) * factor;
                }

                var checksum = Convert.ToInt32(cleanedIsbn13[12]);
                if ((10 - total % 10) % 10 != checksum)
                {
                    throw new IsbnException(
                        $"Error converting ISBN 10 to ISBN 13, checksum error, checksum: {(10 - total % 10) % 10} != {checksum}");
                }

                if (!cleanedIsbn13.StartsWith("978"))
                {
                    throw new IsbnException(
                        $"Error converting ISBN 10 to ISBN 13, invalid ISBN 13 prefix, ISBN 13 {cleanedIsbn13}");
                }

                var isbn10 = cleanedIsbn13.Substring(3, 9);
                total = 0;
                for (var x = 0; x < 8; x++)
                {
                    total += Convert.ToInt32(isbn10[x]) * (10 - x);
                }

                checksum = (11 - total % 11) % 11;
                isbn10 = $"{isbn10}{(checksum == 10 ? "X" : $"{checksum}")}";
                Log.DebugFormat("converted {0} --> {1}, new ISBN 10 is valid? {2}", isbn13, isbn10,
                    IsValidateIsbn(isbn10));
                return isbn10;
            }

            Log.WarnFormat("Invalid ISBN 13: '{0}'", isbn13);
            throw new IsbnException($"Invalid ISBN 13 length, ISBN = '{isbn13}'", "InvalidIsbn13Length");
        }

        public static string CleanIsbn(string isbn)
        {
            if (string.IsNullOrEmpty(isbn))
            {
                throw new IsbnException("ISBN is null or empty", "NullorEmpty");
            }

            return isbn.Replace("-", string.Empty).Replace(" ", string.Empty);
        }

        /// <summary>
        /// Logic that will validate the isbn, making sure that it conforms to the internationally defined standard format of an ISBN.  Use RegEx for this.
        /// http://www.regexlib.com/(A(bX0gu7eaQW4XW0EGUdb7rjoZUMePQd8wxI8H-I3GMYIs8QUzwX0HClMdYohnr-kBTOSggRsTTpJk30y5LOe83sUZJIPvVsoPAWZkyKYJsrgAVIRnlObcaTbleCHV7ACZneurNzosxRQ-_eVhefimhZMt4grc47-RlE79dkWjgGEcT6ZrEF2C2cVP-m4Ugbaj0))/Search.aspx?k=isbn&c=-1&m=-1&ps=20
        //  Expression = ^(97(8|9))?\d{9}(\d|X)$
        //  How to use REGEX with C# - http://www.c-sharpcorner.com/UploadFile/prasad_1/RegExpPSD12062005021717AM/RegExpPSD.aspx
        /// </summary>
        /// <returns>bool</returns>
        public static bool IsValidateIsbn(string isbn)
        {
            if (string.IsNullOrEmpty(isbn))
            {
                throw new IsbnException("ISBN is null or empty", "NullorEmpty");
            }

            //what's the @ sign for?  to escape the \ character.  http://stackoverflow.com/questions/1558058/bad-compile-constant-value
            var isbnRegex = new Regex(@"^(97(8|9))?\d{9}(\d|X)$");
            return isbnRegex.IsMatch(isbn);
        }
    }
}