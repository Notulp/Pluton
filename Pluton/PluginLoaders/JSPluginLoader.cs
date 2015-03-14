using System;
using System.IO;
using System.Collections.Generic;

namespace Pluton
{
    public class JSPluginLoader : Singleton<JSPluginLoader>, ISingleton, IPluginLoader
    {
        public PluginType Type = PluginType.JavaScript;
        public readonly string Extension = ".js";

        public JSPluginLoader()
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
            foreach (DirectoryInfo dirInfo in PluginLoader.GetInstance().pluginDirectory.GetDirectories()) {
                string path = Path.Combine(dirInfo.FullName, dirInfo.Name + Extension);
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
        }

        public void LoadPlugin(string name)
        {
            Logger.LogDebug("[JSPluginLoader] Loading plugin " + name + ".");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name)) {
                Logger.LogError("[JSPluginLoader] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[JSPluginLoader] " + name + " plugin is already loaded.");
            }

            try {
                string code = GetSource(name);
                DirectoryInfo path = new DirectoryInfo(Path.Combine(PluginLoader.GetInstance().pluginDirectory.FullName, name));
                BasePlugin plugin = new JSPlugin(name, code, path);

                PluginLoader.GetInstance().InstallHooks(plugin);
                PluginLoader.GetInstance().Plugins.TryAdd(name, plugin);

                Logger.Log("[JSPluginLoader] " + name + " plugin was loaded successfuly.");
            } catch (Exception ex) {
                Server.GetInstance().Broadcast(name + " plugin could not be loaded.");
                Logger.Log("[JSPluginLoader] " + name + " plugin could not be loaded.");
                Logger.LogException(ex);
            }
        }

        public void LoadPlugins()
        {
            if (CoreConfig.GetInstance().GetBoolValue("javascript", "enabled")) {
                foreach (string name in GetPluginNames())
                    LoadPlugin(name);
            } else {
                Logger.LogDebug("[JSPluginLoader] Javascript plugins are disabled in Core.cfg.");
            }
        }

        public void ReloadPlugin(string name)
        {
            if (PluginLoader.GetInstance().Plugins.ContainsKey(name)) {
                UnloadPlugin(name);
                LoadPlugin(name);
            }
        }

        public void ReloadPlugins()
        {
            foreach (BasePlugin plugin in PluginLoader.GetInstance().Plugins.Values) {
                if (plugin.Type == Type) {
                    UnloadPlugin(plugin.Name);
                    LoadPlugin(plugin.Name);
                }
            }
        }

        public void UnloadPlugin(string name)
        {
            Logger.LogDebug("[JSPluginLoader] Unloading " + name + " plugin.");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name)) {
                BasePlugin plugin = PluginLoader.GetInstance().Plugins[name];

                plugin.KillTimers();
                PluginLoader.GetInstance().RemoveHooks(plugin);
                PluginLoader.GetInstance().Plugins.TryRemove(name, out plugin);

                Logger.LogDebug("[JSPluginLoader] " + name + " plugin was unloaded successfuly.");
            } else {
                Logger.LogError("[JSPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[JSPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
            }
        }

        public void UnloadPlugins()
        {
            foreach (string name in PluginLoader.GetInstance().Plugins.Keys)
                UnloadPlugin(name);
        }

        public void Initialize()
        {
            PluginWatcher.GetInstance().AddWatcher(Type, Extension);
            PluginLoader.GetInstance().PluginLoaders.Add(Type, this);
        }
    }
}

