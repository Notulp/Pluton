using System;
using System.IO;
using System.Reflection;

namespace Pluton.Patcher
{
	class MainClass
	{
		public static int Main (string[] args)
		{
			bool input = true;
			int result = 0;

			Console.WriteLine("Hello World!");
			foreach (string arg in args) {
				if (arg == "--no-input")
					input = false;
			}

			string path = Assembly.GetExecutingAssembly().Location;
			if (path == "")
				result = 1;

			try {
				string file = Path.Combine(path, "Test.log");
				if (File.Exists(file))
					File.Delete(file);
				File.AppendAllText(file, "The updater works like charm!");
			} catch (Exception ex) {
				Console.WriteLine(ex.StackTrace);
				result = 2;
			}


			if (input)
				Console.ReadKey();
			return result;

			/* Result:
			 * 0: OK
			 * 1: Application loaded from memory, can't specify the path.
			 * 2: Error while creating the file, no permissions maybe?
			 * 
			 */
		}
	}
}
