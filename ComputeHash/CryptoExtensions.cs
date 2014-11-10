using System;
using System.Text;
using System.Security.Cryptography;

namespace ComputeHash
{
	public static class CryptoExtensions
	{
		public static string GetMD5Hash(this byte[] input)
		{
			using (MD5 md5hash = MD5.Create()) {
				return GetMD5Hash(md5hash, input);
			}
		}

		public static string GetMD5Hash(MD5 md5Hash, byte[] input)
		{
			byte[] data = md5Hash.ComputeHash(input);

			StringBuilder sBuilder = new StringBuilder();

			for (int i = 0; i < data.Length; i++)
			    sBuilder.Append(data[i].ToString("x2"));

			return sBuilder.ToString();
		}
	}
}

