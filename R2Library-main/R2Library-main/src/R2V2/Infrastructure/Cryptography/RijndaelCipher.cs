#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace R2V2.Infrastructure.Cryptography
{
    public class RijndaelCipher
    {
        private string _initializationVector;
        private byte[] _initVectorBytes = { };
        private string _saltValue;
        private byte[] _saltValueBytes = { };


        public RijndaelCipher()
        {
            PassPhrase = "Eis$JuQJ&O$;K#^TZnhT:Y#X&yl";
            SaltValue = "UkBT,!$*.unSyx'%c=+J!dfAzsS";
            HashAlgorithm = "SHA1";
            PasswordIterations = 2;
            InitializationVector = "=acQm,iV,'%o>eOU";
            KeySize = 256;
        }


        /// <summary>
        ///     Passphrase can be any string. Passphrase from which a pseudo-random password will be derived. The
        ///     derived password will be used to generate the encryption key.
        /// </summary>
        public string PassPhrase { get; set; }

        /// <summary>
        ///     Salt value can be any string.  Salt value used along with passphrase to generate password.
        /// </summary>
        public string SaltValue
        {
            get => _saltValue;
            set
            {
                _saltValue = value;

                // Convert strings into byte arrays.
                // Let us assume that strings only contain ASCII codes.
                // If strings include Unicode characters, use Unicode, UTF7, or UTF8
                // encoding.
                _saltValueBytes = Encoding.ASCII.GetBytes(_saltValue);
            }
        }

        /// <summary>
        ///     Hash algorithm used to generate password. Allowed values are: "MD5" and
        ///     "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
        /// </summary>
        public string HashAlgorithm { get; set; }

        /// <summary>
        ///     Number of iterations used to generate password. One or two iterations
        ///     should be enough.
        /// </summary>
        public int PasswordIterations { get; set; }

        /// <summary>
        ///     Initialization vector (or IV). This value is required to encrypt the
        ///     first block of plaintext data. For RijndaelManaged class IV must be
        ///     exactly 16 ASCII characters long.
        /// </summary>
        public string InitializationVector
        {
            get => _initializationVector;
            set
            {
                _initializationVector = value;

                // Convert strings into byte arrays.
                // Let us assume that strings only contain ASCII codes.
                // If strings include Unicode characters, use Unicode, UTF7, or UTF8
                // encoding.
                _initVectorBytes = Encoding.ASCII.GetBytes(_initializationVector);
            }
        }


        /// <summary>
        ///     Size of encryption key in bits. Allowed values are: 128, 192, and 256.
        ///     Longer keys are more secure than shorter keys.
        /// </summary>
        public int KeySize { get; set; }

        /// <summary>
        ///     Encrypts a string using Rijndael
        /// </summary>
        public string Encrypt(string plainText)
        {
            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and
            // salt value. The password will be created using the specified hash
            // algorithm. Password creation can be done in several iterations.
            //PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(PassPhrase, saltValueBytes, HashAlgorithm, PasswordIterations);
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(PassPhrase, _saltValueBytes, PasswordIterations))
            {
                // Use the password to generate pseudo-random bytes for the encryption
                // key. Specify the size of the key in bytes (instead of bits).
                //passwordDeriveBytes.
                //byte[] keyBytes = passwordDeriveBytes.GetBytes(KeySize / 8);
                var keyBytes = rfc2898DeriveBytes.GetBytes(KeySize / 8);

                // Create uninitialized Rijndael encryption object.
                using (var symmetricKey = new RijndaelManaged())
                {
                    // It is reasonable to set encryption mode to Cipher Block Chaining
                    // (CBC). Use default options for other symmetric key parameters.
                    symmetricKey.Mode = CipherMode.CBC;

                    // Generate encryptor from the existing key bytes and initialization
                    // vector. Key size will be defined based on the number of the key
                    // bytes.
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, _initVectorBytes))
                    {
                        // Define memory stream which will be used to hold encrypted data.
                        using (var memoryStream = new MemoryStream())
                        {
                            // Define cryptographic stream (always use Write mode for encryption).
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                // Start encrypting.
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

                                // Finish encrypting.
                                cryptoStream.FlushFinalBlock();

                                // Convert our encrypted data from a memory stream into a byte array.
                                var cipherTextBytes = memoryStream.ToArray();

                                // Close both streams.
                                memoryStream.Close();
                                cryptoStream.Close();

                                // Convert encrypted data into a base64-encoded string.
                                var cipherText = Convert.ToBase64String(cipherTextBytes);

                                // Return encrypted string.
                                return cipherText;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Decrypts a string using Rijndael
        /// </summary>
        public string Decrypt(string cipherText)
        {
            // Convert our ciphertext into a byte array.
            var cipherTextBytes = Convert.FromBase64String(cipherText);

            // First, we must create a password, from which the key will be
            // derived. This password will be generated from the specified
            // passphrase and salt value. The password will be created using
            // the specified hash algorithm. Password creation can be done in
            // several iterations.
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(PassPhrase, _saltValueBytes, PasswordIterations))
            {
                // Use the password to generate pseudo-random bytes for the encryption
                // key. Specify the size of the key in bytes (instead of bits).
                var keyBytes = rfc2898DeriveBytes.GetBytes(KeySize / 8);

                // Create uninitialized Rijndael encryption object.
                using (var symmetricKey = new RijndaelManaged())
                {
                    // It is reasonable to set encryption mode to Cipher Block Chaining
                    // (CBC). Use default options for other symmetric key parameters.
                    symmetricKey.Mode = CipherMode.CBC;

                    // Generate decryptor from the existing key bytes and initialization
                    // vector. Key size will be defined based on the number of the key
                    // bytes.
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, _initVectorBytes))
                    {
                        // Define memory stream which will be used to hold encrypted data.
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            // Define cryptographic stream (always use Read mode for encryption).
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                // Since at this point we don't know what the size of decrypted data
                                // will be, allocate the buffer long enough to hold ciphertext;
                                // plaintext is never longer than ciphertext.
                                var plainTextBytes = new byte[cipherTextBytes.Length];

                                // Start decrypting.
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                                // Close both streams.
                                memoryStream.Close();
                                cryptoStream.Close();

                                // Convert decrypted data into a string.
                                // Let us assume that the original plaintext string was UTF8-encoded.
                                var plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

                                // Return decrypted string.
                                return plainText;
                            }
                        }
                    }
                }
            }
        }
    }
}