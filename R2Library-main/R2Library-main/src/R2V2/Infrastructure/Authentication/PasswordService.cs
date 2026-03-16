#region

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace R2V2.Infrastructure.Authentication
{
    public class PasswordService
    {
        private const int Pbkdf2IterCount = 2000; // default for Rfc2898DeriveBytes
        private const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
        private const int SaltSize = 192 / 8; // 192 bits

        public static string GenerateNewSalt()
        {
            //Generate a cryptographic random number.
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[SaltSize];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        public static string GeneratePasswordHash(string password, string salt)
        {
            var hashAsBytes = GenerateSaltedHash(password, salt);
            var passwordHash = Convert.ToBase64String(hashAsBytes);
            return passwordHash;
        }

        private static byte[] GenerateSaltedHash(string password, string salt)
        {
            var passwordAsBytes = Encoding.UTF8.GetBytes(password);
            var saltAsBytes = Encoding.UTF8.GetBytes(salt);

            var hashAsBytes = GenerateSaltedHash(passwordAsBytes, saltAsBytes);

            return hashAsBytes;
        }

        private static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            var plainTextWithSaltBytes =
                new byte[plainText.Length + salt.Length];

            for (var i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }

            for (var i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public static bool IsPasswordCorrect(string password, string hashedPassword, string salt)
        {
            var hashedPasswordAsBytes = Convert.FromBase64String(hashedPassword);
            var passwordHashAsBytes = GenerateSaltedHash(password, salt);
            return CompareByteArrays(hashedPasswordAsBytes, passwordHashAsBytes);
        }

        private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            return !array1.Where((t, i) => t != array2[i]).Any();
        }

        public static string GenerateSlowPasswordHash(string password, string salt)
        {
            var saltAsBytes = Encoding.UTF8.GetBytes(salt);
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltAsBytes, Pbkdf2IterCount))
            {
                //salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(Pbkdf2SubkeyLength);
            }


            var outputBytes = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
            Buffer.BlockCopy(saltAsBytes, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, Pbkdf2SubkeyLength);
            return Convert.ToBase64String(outputBytes);
        }

        public static bool IsSlowPasswordCorrect(string password, string hashedPassword, string salt)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword) ||
                string.IsNullOrWhiteSpace(salt))
            {
                return false;
            }

            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Wrong length or version header.
            if (hashedPasswordBytes.Length != 1 + SaltSize + Pbkdf2SubkeyLength || hashedPasswordBytes[0] != 0x00)
            {
                return false;
            }

            //byte[] salt = new byte[SaltSize];
            var saltAsBytes = Encoding.UTF8.GetBytes(salt);

            Buffer.BlockCopy(hashedPasswordBytes, 1, saltAsBytes, 0, SaltSize);
            var storedSubkey = new byte[Pbkdf2SubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + SaltSize, storedSubkey, 0, Pbkdf2SubkeyLength);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltAsBytes, Pbkdf2IterCount))
            {
                generatedSubkey = deriveBytes.GetBytes(Pbkdf2SubkeyLength);
            }

            return storedSubkey.SequenceEqual(generatedSubkey);
        }
    }
}