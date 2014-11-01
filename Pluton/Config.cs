using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

namespace Pluton
{
    public class Config
    {
        public static IniParser PlutonConfig;

        public static void Init()
        {
            string ConfigPath = Path.Combine(Util.GetPublicFolder(), "Pluton.cfg");

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

        public static string GetValue(string Section, string Setting, string defaultValue = "")
        {
            if (!PlutonConfig.ContainsSetting(Section, Setting)) {
                PlutonConfig.AddSetting(Section, Setting, defaultValue);
                PlutonConfig.Save();
            }
            return PlutonConfig.GetSetting(Section, Setting, defaultValue);
        }

        public static bool GetBoolValue(string Section, string Setting, bool defaultValue = false)
        {
            if (!PlutonConfig.ContainsSetting(Section, Setting)) {
                PlutonConfig.AddSetting(Section, Setting, defaultValue.ToString().ToLower());
                PlutonConfig.Save();
            }
            return PlutonConfig.GetBoolSetting(Section, Setting, defaultValue);
        }

        public static void Reload()
        {
            string ConfigPath = Path.Combine(Util.GetPublicFolder(), "Pluton.cfg");
             
            if (File.Exists(ConfigPath))
                PlutonConfig = new IniParser(ConfigPath);
        }
    }
}

