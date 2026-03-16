using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ContentSigner
{
    internal class Program
    {
        private static IDictionary<string, string> _hashes;
        private static string _root;
        private const string _signatureMark = ".sig-";

        private static void Main(string[] args)
        {
            _root = args[0];
            var outputFile = args[1];

            string[] filters = new string[0];
            if (args.Length == 3)
            {
                filters = args[2].Split('|');
            }

            _hashes = new Dictionary<string, string>();
            ProcessDirectory(_root, filters);
            WriteFile(outputFile);
        }

        private static void ProcessDirectory(string directoryPath, string[] filters)
        {
            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                if (directory.Contains(".svn"))
                {
                    continue;
                }
                ProcessDirectory(directory, filters);
            }
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                if (file.Contains(_signatureMark))
                {
                    File.Delete(file);
                    continue;
                }
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                if (filters != null && filters.Count() > 0 && !filters.Any(x => file.EndsWith(x)))
                {
                    continue;
                }

                var assetName = file.Remove(0, _root.Length + 1).Replace('\\', '/');
                var signature = CreateSignature(file);

                var signedFile = CreateSignedFile(file, signature);
                var signedAssentName = signedFile.Remove(0, _root.Length + 1).Replace('\\', '/');

                _hashes.Add(assetName, signedAssentName);
            }
        }

        private static string CreateSignedFile(string file, string signature)
        {

            var signedFileName = file.Insert(file.LastIndexOf("."), string.Concat(_signatureMark, signature));
            File.Copy(file, signedFileName);

            return signedFileName;

        }

        private static string CreateSignature(string file)
        {
            byte[] bytes;
            using (var hash = new Crc32())
            {
                bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(File.ReadAllText(file)));
            }

            var data = new StringBuilder();
            Array.ForEach(bytes, b => data.Append(b.ToString("x2")));
            return data.ToString();
        }

        private static void WriteFile(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                foreach (var kvp in _hashes)
                {
                    sw.WriteLine("{0}|{1}", kvp.Key, kvp.Value);
                }
            }
        }
    }
}