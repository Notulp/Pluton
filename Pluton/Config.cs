using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Pluton {
	public class Config {
		public static IniParser PlutonDirectoryConfig;
		public static IniParser PlutonConfig;

		public static void Init(string DirectoryConfigPath) {
			if (File.Exists(DirectoryConfigPath)) {
				PlutonDirectoryConfig = new IniParser(DirectoryConfigPath);
				Debug.Log("DirectoryConfig " + DirectoryConfigPath + " loaded!");
			}
			else Debug.Log("DirectoryConfig " + DirectoryConfigPath + " NOT loaded!");

			string ConfigPath = Path.Combine(GetPublicFolder(), "Pluton.cfg");

			if (File.Exists(ConfigPath)) {
				PlutonConfig = new IniParser(ConfigPath);
				Debug.Log("Config " + ConfigPath + " loaded!");
			}
			else Debug.Log("Config " + ConfigPath + " NOT loaded!");
		}

		public static string GetValue(string Section, string Setting) {
			return PlutonConfig.GetSetting(Section, Setting);
		}

		public static bool GetBoolValue(string Section,string Setting) {
			var val = PlutonConfig.GetSetting(Section, Setting);
			return val != null && val.ToLower() == "true";
		}

		public static string GetModulesFolder() {
			Regex root = new Regex(@"^%RootFolder%", RegexOptions.IgnoreCase);             
			string path = root.Replace(PlutonDirectoryConfig.GetSetting("Settings", "ModulesFolder"), Util.GetRootFolder()) + @"\";
			return Util.NormalizePath(path);
		}

		public static string GetPublicFolder() {
			Regex root = new Regex(@"^%RootFolder%", RegexOptions.IgnoreCase);             
			string path = root.Replace(PlutonDirectoryConfig.GetSetting("Settings", "PublicFolder"), Util.GetRootFolder()) + @"\";
			return Util.NormalizePath(path);
		}
	}
}

