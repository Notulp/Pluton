using System;
using System.IO;
using System.Collections.Generic;
using Pluton.PluginLoaders;

namespace Pluton
{
    public class PHPPluginLoader : Singleton<PHPPluginLoader>, ISingleton, IPluginLoader
    {
        public PluginType Type = PluginType.PHP;
        public readonly string Extension = ".php";

        public PHPPluginLoader()
        {
        }

        public string GetExtension()
        {
            return Extension;
        }

        public string GetSource(string pluginname)
        {
            return File.ReadAllText(GetMainFilePath(pluginname));
        }

        public string GetMainFilePath(string pluginname)
        {
            return Path.Combine(GetPluginDirectoryPath(pluginname), pluginname + Extension);
        }

        public string GetPluginDirectoryPath(string name)
        {
            return Path.Combine(PluginLoader.GetInstance().pluginDirectory.FullName, name);
        }

        public System.Collections.Generic.IEnumerable<string> GetPluginNames()
        {
            foreach (DirectoryInfo dirInfo in PluginLoader.GetInstance().pluginDirectory.GetDirectories())
            {
                string path = Path.Combine(dirInfo.FullName, dirInfo.Name + Extension);
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
        }

        public void LoadPlugin(string name)
        {
            Logger.LogDebug("[PHPPluginLoader] Loading plugin " + name + ".");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name))
            {
                Logger.LogError("[PHPPluginLoader] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[PHPPluginLoader] " + name + " plugin is already loaded.");
            }

            try
            {
                string code = GetSource(name);
                DirectoryInfo path = new DirectoryInfo(Path.Combine(PluginLoader.GetInstance().pluginDirectory.FullName, name));
                new PHPPlugin(name, code, path);

            }
            catch (Exception ex)
            {
                Server.GetInstance().Broadcast(name + " plugin could not be loaded.");
                Logger.Log("[PHPPluginLoader] " + name + " plugin could not be loaded.");
                Logger.LogException(ex);
            }
        }

        public void LoadPlugins()
        {
            if (CoreConfig.GetInstance().GetBoolValue("php", "enabled"))
            {
                foreach (string name in GetPluginNames())
                {
                    LoadPlugin(name);
                }
            }
            else
            {
                Logger.LogDebug("[PHPPluginLoader] PHP plugins are disabled in Core.cfg.");
            }
        }

        public void ReloadPlugin(string name)
        {
            if (PluginLoader.GetInstance().Plugins.ContainsKey(name))
            {
                UnloadPlugin(name);
                LoadPlugin(name);
            }
        }

        public void ReloadPlugins()
        {
            foreach (BasePlugin plugin in PluginLoader.GetInstance().Plugins.Values)
            {
                if (!plugin.DontReload)
                {
                    if (plugin.Type == Type)
                    {
                        UnloadPlugin(plugin.Name);
                        LoadPlugin(plugin.Name);
                    }
                }
            }
        }

        public void UnloadPlugin(string name)
        {
            Logger.LogDebug("[PHPPluginLoader] Unloading " + name + " plugin.");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name))
            {
                BasePlugin plugin = PluginLoader.GetInstance().Plugins[name];
                if (plugin.DontReload)
                    return;

                if (plugin.Globals.Contains("On_PluginDeinit"))
                    plugin.Invoke("On_PluginDeinit");

                plugin.KillTimers();
                PluginLoader.GetInstance().RemoveHooks(plugin);
                PluginLoader.GetInstance().Plugins.TryRemove(name, out plugin);

                Logger.LogDebug("[PHPPluginLoader] " + name + " plugin was unloaded successfuly.");
            }
            else
            {
                Logger.LogError("[PHPPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[PHPPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
            }
        }

        public void UnloadPlugins()
        {
            foreach (string name in PluginLoader.GetInstance().Plugins.Keys)
                UnloadPlugin(name);
        }

        public void Initialize()
        {
            Logger.LogWarning("Initializing");
            PluginWatcher.GetInstance().AddWatcher(Type, Extension);
            PluginLoader.GetInstance().PluginLoaders.Add(Type, this);
            Logger.LogWarning("loading");
            LoadPlugins();
        }

        public bool CheckDependencies()
        {
            return CoreConfig.GetInstance().GetBoolValue("php", "enabled") &&
            File.Exists(Path.Combine(Path.Combine(Util.GetServerFolder(), "Managed"), "PhpNetCore.dll")) &&
            File.Exists(Path.Combine(Path.Combine(Util.GetServerFolder(), "Managed"), "PhpNetClassLibrary.dll"));
        }
    }
}

