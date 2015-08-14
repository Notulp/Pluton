using System.IO;
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

        public bool CheckDependencies()
        {
            return SingletonEx.IsInitialzed<DirectoryConfig>();
        }

        public void GenerateConfig()
        {
            ConfigFile.AddSetting("csharp", "enabled", "true");
            ConfigFile.AddSetting("csharp", "checkHash", "true");

            ConfigFile.AddSetting("csscript", "enabled", "true");
            ConfigFile.AddSetting("csscript", "checkHash", "true");

            ConfigFile.AddSetting("python", "enabled", "true");
            ConfigFile.AddSetting("python", "checkHash", "false");

            ConfigFile.AddSetting("javascript", "enabled", "true");
            ConfigFile.AddSetting("javascript", "checkHash", "false");
            
            ConfigFile.AddSetting("lua", "enabled", "true");
            ConfigFile.AddSetting("lua", "checkHash", "false");
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

