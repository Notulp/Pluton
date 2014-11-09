using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Pluton
{
    public static class CryptoExtensions
    {
        private static List<string> TrustedHashes;

        public static void Init()
        {
            TrustedHashes = new List<string>();
            string path = DirectoryConfig.GetConfigPath("Hashes");
            TrustedHashes = (from line in File.ReadAllLines(path)
                             where !String.IsNullOrEmpty(line) && !line.StartsWith("//")
                             select line).ToList<string>();
            foreach (string hash in TrustedHashes) {
                Console.WriteLine("HASH: " + hash);
            }

        }

        public static string GetMD5Hash(MD5 md5Hash, string input)
        {
            return GetMD5Hash(md5Hash, Encoding.UTF8.GetBytes(input));
        }

        public static string GetMD5Hash(MD5 md5Hash, byte[] input)
        {
            byte[] data = md5Hash.ComputeHash(input);

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }

        public static bool VerifyMD5Hash(this string input)
        {
            using (MD5 md5Hash = MD5.Create()) {
                return TrustedHashes.Contains(GetMD5Hash(md5Hash, input));
            }
        }

        public static bool VerifyMD5Hash(this byte[] input)
        {
            using (MD5 md5Hash = MD5.Create()) {
                foreach (string hash in TrustedHashes) {
                    Console.WriteLine("HASH: " + hash + " : " + GetMD5Hash(md5Hash, input));
                }

                return TrustedHashes.Contains(GetMD5Hash(md5Hash, input));
            }
        }

        public static bool VerifyMD5Hash(this string input, string hash)
        {
            using (MD5 md5hash = MD5.Create()) {
                return VerifyMD5Hash(md5hash, input, hash);
            }
        }

        public static bool VerifyMD5Hash(this byte[] input, string hash)
        {
            using (MD5 md5hash = MD5.Create()) {
                return VerifyMD5Hash(md5hash, input, hash);
            }
        }

        public static bool VerifyMD5Hash(MD5 md5Hash, string input, string hash)
        {
            return VerifyMD5Hash(md5Hash, Encoding.UTF8.GetBytes(input), hash);
        }

        public static bool VerifyMD5Hash(MD5 md5Hash, byte[] input, string hash)
        {
            string hashOfInput = GetMD5Hash(md5Hash, input);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
                return true;
            else
                return false;
        }
    }
}

