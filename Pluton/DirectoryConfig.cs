using System.IO;
using UnityEngine;

namespace Pluton
{
    public class DirectoryConfig : Singleton<DirectoryConfig>, ISingleton
    {
        IniParser DirConfig;

        public void Initialize()
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

        public bool CheckDependencies()
        {
            return true;
        }

        public string GetConfigPath(string config)
        {
            string path = DirConfig.GetSetting("Directories", config);

            if (path.StartsWith("%public%", System.StringComparison.Ordinal))
                path = path.Replace("%public%", Util.GetPublicFolder());

            if (path.StartsWith("%data%", System.StringComparison.Ordinal))
                path = path.Replace("%data%", Util.GetServerFolder());

            if (path.StartsWith("%root%", System.StringComparison.Ordinal))
                path = path.Replace("%root%", Util.GetRootFolder());

            if (path.StartsWith("%identity%", System.StringComparison.Ordinal))
                path = path.Replace("%identity%", Util.GetIdentityFolder());

            return path;
        }

        public void Reload()
        {
            string ConfigPath = Path.Combine(Util.GetServerFolder(), "DirectoryConfig.cfg");

            if (File.Exists(ConfigPath))
                DirConfig = new IniParser(ConfigPath);
        }
    }
}

