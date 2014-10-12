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
                pluton.enabled = GetBoolValue("Config", "enabled", true);
               
            } else {
                Debug.Log("Config " + ConfigPath + " NOT loaded!");
                Debug.Log("Disabling pluton!");
                pluton.enabled = false;
            }
        }

        public static string GetValue(string Section, string Setting, string defaultValue = "")
        {
            return PlutonConfig.GetSetting(Section, Setting, defaultValue);
        }

        public static bool GetBoolValue(string Section, string Setting, bool defaultValue = false)
        {
            return PlutonConfig.GetBoolSetting(Section, Setting, defaultValue);
        }
            
    }
}

