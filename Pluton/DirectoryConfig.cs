using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

namespace Pluton
{
    public class DirectoryConfig
    {
        private static IniParser DirConfig;

        public static void Init()
        {
            string ConfigPath = Path.Combine(Util.GetServerFolder(), "DirectoryConfig.cfg");

            if (File.Exists(ConfigPath)) {
                DirConfig = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " loaded!");
            } else {
                char sc = Path.DirectorySeparatorChar;
                Directory.CreateDirectory(Util.GetPublicFolder());
                File.Create(ConfigPath).Close();
                DirConfig = new IniParser(ConfigPath);
                Debug.Log("Config " + ConfigPath + " Created!");
                Debug.Log("The config will be filled with the default values.");
                DirConfig.AddSetting("Directories", "Core", "%data%" + sc + "Core.cfg");
                DirConfig.AddSetting("Directories", "Pluton", "%public%" + sc + "Pluton.cfg");
                DirConfig.AddSetting("Directories", "Hashes", "%data%" + sc + "Hashes.ini");
                DirConfig.Save();
            }
        }

        public static string GetConfigPath(string config)
        {
            string path = DirConfig.GetSetting("Directories", config);

            if (path.StartsWith("%public%"))
                path = path.Replace("%public%", Util.GetPublicFolder());

            if (path.StartsWith("%data%"))
                path = path.Replace("%data%", Util.GetServerFolder());

            if (path.StartsWith("%root%"))
                path = path.Replace("%root%", Util.GetRootFolder());

            if (path.StartsWith("%identity%"))
                path = path.Replace("%identity%", Util.GetIdentityFolder());

            return path;
        }

        public static void Reload()
        {
            string ConfigPath = Path.Combine(Util.GetPublicFolder(), "DirectoryConfig.cfg");

            if (File.Exists(ConfigPath))
                DirConfig = new IniParser(ConfigPath);
        }
    }
}

