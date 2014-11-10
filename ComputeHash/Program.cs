using System;
using System.IO;

namespace ComputeHash
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hashes.txt");

			string[] lines;
			if (File.Exists(path))
				lines = File.ReadAllLines(path);
			else
				lines = new string[]{};

			foreach (string arg in args) {
				string hash = File.ReadAllBytes(arg).GetMD5Hash();

				bool contains = false;
				foreach (string line in lines) {
					if (line.StartsWith("//"))
						continue;
					if (line.Equals(hash)) {
						contains = true;
						break;
					}
				}

				if (!contains) {
					File.AppendAllText(path, String.Format("// {0}\r\n{1}\r\n\r\n", Path.GetFileName(arg), hash));
					Console.WriteLine("The file's hash has been successfully added to: " + path);
				} else {
					Console.WriteLine("The file's hash is already added to: " + path);
				}
			}
			Console.ReadKey();
		}
	}
}
