using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

namespace Pluton
{
    public class CoreConfig : Singleton<CoreConfig>, ISingleton
    {
        private IniParser ConfigFile;

        public void Initialize()
        {
            string ConfigPath = DirectoryConfig.GetInstance().GetConfigPath("Core");

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

        public void GenerateConfig()
        {
            ConfigFile.AddSetting("csharp", "enabled", "false");
            ConfigFile.AddSetting("csharp", "checkHash", "true");

            ConfigFile.AddSetting("csscript", "enabled", "false");
            ConfigFile.AddSetting("csscript", "checkHash", "true");

            ConfigFile.AddSetting("python", "enabled", "true");
            ConfigFile.AddSetting("python", "checkHash", "false");

            ConfigFile.AddSetting("javascript", "enabled", "true");
            ConfigFile.AddSetting("javascript", "checkHash", "false");
        }

        public string GetValue(string Section, string Setting)
        {
            return ConfigFile.GetSetting(Section, Setting);
        }

        public bool GetBoolValue(string Section, string Setting)
        {
            return ConfigFile.GetBoolSetting(Section, Setting);
        }

        public void Reload()
        {
            string ConfigPath = DirectoryConfig.GetInstance().GetConfigPath("Core");

            if (File.Exists(ConfigPath))
                ConfigFile = new IniParser(ConfigPath);
        }
    }
}

