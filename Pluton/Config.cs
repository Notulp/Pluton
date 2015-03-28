using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

namespace Pluton
{
    public class Config : Singleton<Config>, ISingleton
    {
        public IniParser PlutonConfig;

        public void Initialize()
        {
            string ConfigPath = DirectoryConfig.GetInstance().GetConfigPath("Pluton");

            if (File.Exists(ConfigPath)) {
                PlutonConfig = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " loaded!");
            } else {
                Directory.CreateDirectory(Util.GetPublicFolder());
                File.Create(ConfigPath).Close();
                PlutonConfig = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " Created!");
                Debug.Log("The config will be filled with the default values.");
            }
            pluton.enabled = GetBoolValue("Config", "enabled", true);
        }

        public bool CheckDependencies()
        {
            return SingletonEx.IsInitialzed<DirectoryConfig>() &&
                File.Exists(DirectoryConfig.GetInstance().GetConfigPath("Pluton"));
        }

        public string GetValue(string Section, string Setting, string defaultValue = "")
        {
            if (!PlutonConfig.ContainsSetting(Section, Setting)) {
                PlutonConfig.AddSetting(Section, Setting, defaultValue);
                PlutonConfig.Save();
            }
            return PlutonConfig.GetSetting(Section, Setting, defaultValue);
        }

        public bool GetBoolValue(string Section, string Setting, bool defaultValue = false)
        {
            if (!PlutonConfig.ContainsSetting(Section, Setting)) {
                PlutonConfig.AddSetting(Section, Setting, defaultValue.ToString().ToLower());
                PlutonConfig.Save();
            }
            return PlutonConfig.GetBoolSetting(Section, Setting, defaultValue);
        }

        public void Reload()
        {
            string ConfigPath = DirectoryConfig.GetInstance().GetConfigPath("Pluton");
             
            if (File.Exists(ConfigPath))
                PlutonConfig = new IniParser(ConfigPath);
        }
    }
}

