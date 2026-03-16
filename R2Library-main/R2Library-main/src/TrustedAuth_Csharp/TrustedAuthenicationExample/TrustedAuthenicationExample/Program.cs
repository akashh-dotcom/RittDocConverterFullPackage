using System;

namespace TrustedAuthenicationExample
{
    class Program
    {
        static void Main()
        {
            bool exitProgram = false;
            while (!exitProgram)
            {
                exitProgram = RunProcess();
            }

        }

        public static bool RunProcess()
        {
            //Console.WriteLine("Please enter your Account #");
            //string accountNumber = Console.ReadLine();

            //Console.WriteLine("Please enter your 16 character Security Key");
            //string salt = Console.ReadLine();

            string accountNumber = "005034";
            string r2TrustedSecurityKey = "gu4tPPnyojPFpXdN";
            R2LibraryTrustedAuth trustedAuth = new R2LibraryTrustedAuth();
            string r2Link = string.Format("http://www.R2Library.com/?acctno={0}&timestamp={1}&hash={2}",
                accountNumber, trustedAuth.GetTimeStamp(),
                trustedAuth.GetHashKey(accountNumber, r2TrustedSecurityKey));

            string utcTimestamp = trustedAuth.GetTimeStamp();

            Console.WriteLine("UTC TimeNow: {0}", trustedAuth.UtcTimestamp);
            Console.WriteLine("UTC Timestamp: {0}", utcTimestamp);
            Console.WriteLine("R2Library.com Trusted Authentication Link: {0}", r2Link);

            //string hashKey = trustedAuth.GetHashKey(accountNumber, salt);

            //Console.WriteLine("Your R2 Library Securied URL is:");
            //Console.WriteLine("");
            //Console.WriteLine("http://www.R2Library.com/default.aspx?acctno={0}&timestamp={1}&hash={2}", accountNumber, utcTimestamp, hashKey);
            //Console.WriteLine("");
            //Console.WriteLine("");
            //Console.WriteLine("Press enter to Exit or 1 to rerun");
            //string test = Console.ReadLine();
            //return string.IsNullOrWhiteSpace(test);

            return false;

        }
    }
}
