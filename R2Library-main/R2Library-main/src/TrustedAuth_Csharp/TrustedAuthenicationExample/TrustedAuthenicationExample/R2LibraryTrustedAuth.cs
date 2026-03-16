using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TrustedAuthenicationExample
{
	public class R2LibraryTrustedAuth
	{
		private DateTime _utcTimestamp = DateTime.UtcNow;

		public DateTime UtcTimestamp
		{
			get { return _utcTimestamp; }
		}

		public string GetTimeStamp()
		{
			return string.Format("{0:yyyyMMddHHmmss}", _utcTimestamp);
		}

		public string GetHashKey(string accountNumber, string saltKey)
		{
			string builder = new StringBuilder()
				.Append(accountNumber)
				.Append("|")
				.Append(_utcTimestamp)
				.ToString();

			byte[] bytes = new ASCIIEncoding().GetBytes(saltKey);

			string str = ComputeSha1Hash(builder, bytes);
			return Uri.EscapeUriString(str);
		}

		private static string ComputeSha1Hash(string plaintext, IList<byte> saltBytes)
		{
			Byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintext);
			Byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Count];

			for (int i = 0; i < plainTextBytes.Length; i++)
			{
				plainTextWithSaltBytes[i] = plainTextBytes[i];
			}

			for (int i = 0; i < saltBytes.Count; i++)
			{
				plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
			}

			HashAlgorithm hash = new SHA1Managed();

			Byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

			Byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Count];

			for (int i = 0; i < hashBytes.Length; i++)
			{
				hashWithSaltBytes[i] = hashBytes[i];
			}

			for (int i = 0; i < saltBytes.Count; i++)
			{
				hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
			}
			String hashValue = Convert.ToBase64String(hashWithSaltBytes);

			return hashValue;
		}
	}
}
