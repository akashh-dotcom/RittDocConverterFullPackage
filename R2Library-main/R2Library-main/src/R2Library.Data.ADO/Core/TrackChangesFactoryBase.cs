#region

using System;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace R2Library.Data.ADO.Core
{
    public class TrackChangesFactoryBase : FactoryBase, ITrackChanges
    {
        public string DebugHashInfo { get; private set; }
        public string TrackChangesHash { get; private set; }

        public void SetTrackChangesHash()
        {
            var stringToHash = GetStringToHash();
            TrackChangesHash = CreateMd5Hash(stringToHash);
        }

        public bool IsDirty()
        {
            var stringToHash = GetStringToHash();
            var currentHash = CreateMd5Hash(stringToHash);
            DebugHashInfo = $"currentHash: {currentHash}, entity.TrackChangesHash: {TrackChangesHash}";
            return currentHash != TrackChangesHash;
        }

        protected virtual string GetStringToHash()
        {
            //JsonSerializerSettings settings = new JsonSerializerSettings {MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
            //return JsonConvert.SerializeObject(this, settings);
            return $"this method needs to be implemented in the derived class {DateTime.Now:O}";
        }

        public static string CreateMd5Hash(string textToHash)
        {
            // Use input string to calculate MD5 hash
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(textToHash);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                //vsb.Append(hashBytes[i].ToString("X2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}