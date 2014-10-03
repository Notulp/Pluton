using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
                pluton.enabled = GetBoolValue("Config", "enabled");
            } else {
                Debug.Log("Config " + ConfigPath + " NOT loaded!");
                Debug.Log("Disabling pluton!");
                pluton.enabled = false;
            }
        }

        public static string GetValue(string Section, string Setting)
        {
            return PlutonConfig.GetSetting(Section, Setting);
        }

        public static bool GetBoolValue(string Section, string Setting)
        {
            var val = PlutonConfig.GetSetting(Section, Setting);
            return val != null && val.ToLower() == "true";
        }
    }
}

