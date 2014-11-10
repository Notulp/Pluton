using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

namespace Pluton
{
    public class CoreConfig
    {
        private static IniParser ConfigFile;

        public static void Init()
        {
            string ConfigPath = DirectoryConfig.GetConfigPath("Core");

            if (File.Exists(ConfigPath)) {
                ConfigFile = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " loaded!");
            } else {
                File.Create(ConfigPath).Close();
                ConfigFile = new IniParser(ConfigPath);
                Debug.Log("Core config " + ConfigPath + " Created!");
                Debug.Log("The config will be filled with the default values.");
                GenerateConfig();
                ConfigFile.Save();
            }
        }

        public static void GenerateConfig()
        {
            ConfigFile.AddSetting("csharp", "enabled", "false");
            ConfigFile.AddSetting("csharp", "checkHash", "true");

            ConfigFile.AddSetting("python", "enabled", "true");
            ConfigFile.AddSetting("python", "checkHash", "false");

            ConfigFile.AddSetting("javascript", "enabled", "true");
            ConfigFile.AddSetting("javascript", "checkHash", "false");
        }

        public static string GetValue(string Section, string Setting)
        {
            return ConfigFile.GetSetting(Section, Setting);
        }

        public static bool GetBoolValue(string Section, string Setting)
        {
            return ConfigFile.GetBoolSetting(Section, Setting);
        }

        public static void Reload()
        {
            string ConfigPath = Path.Combine(Util.GetPublicFolder(), "Core.cfg");

            if (File.Exists(ConfigPath))
                ConfigFile = new IniParser(ConfigPath);
        }
    }
}

