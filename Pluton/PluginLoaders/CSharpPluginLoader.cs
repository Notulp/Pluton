﻿using System;
using System.IO;

namespace Pluton
{
    public class CSharpPluginLoader : Singleton<CSharpPluginLoader>, ISingleton, IPluginLoader
    {
        public PluginType Type = PluginType.CSharp;
        public readonly string Extension = ".dll";

        public string GetExtension()
        {
            return Extension;
        }

        public string GetSource(string pluginname)
        {
            return GetMainFilePath(pluginname);
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
            Logger.LogDebug("[CSharpPluginLoader] Loading plugin " + name + ".");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name)) {
                Logger.LogError("[CSharpPluginLoader] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[CSharpPluginLoader] " + name + " plugin is already loaded.");
            }

            if (PluginLoader.GetInstance().CurrentlyLoadingPlugins.Contains(name)) {
                Logger.LogWarning(name + " plugin is already being loaded. Returning.");
                return;
            }

            try {
                string code = GetSource(name);

                var path = new DirectoryInfo(Path.Combine(PluginLoader.GetInstance().pluginDirectory.FullName, name));

                PluginLoader.GetInstance().CurrentlyLoadingPlugins.Add(name);

                new CSPlugin(name, code, path);

            } catch (Exception ex) {
                Server.GetInstance().Broadcast(name + " plugin could not be loaded.");
                Logger.Log("[CSharpPluginLoader] " + name + " plugin could not be loaded.");
                Logger.LogException(ex);
                if (PluginLoader.GetInstance().CurrentlyLoadingPlugins.Contains(name)) {
                    PluginLoader.GetInstance().CurrentlyLoadingPlugins.Remove(name);
                }
            }
        }

        public void LoadPlugins()
        {
            if (CoreConfig.GetInstance().GetBoolValue("csharp", "enabled")) {
                foreach (string name in GetPluginNames())
                    LoadPlugin(name);
            } else {
                Logger.LogDebug("[CSharpPluginLoader] C# plugins are disabled in Core.cfg.");
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
                if (!plugin.DontReload) {
                    if (plugin.Type == Type) {
                        UnloadPlugin(plugin.Name);
                        LoadPlugin(plugin.Name);
                    }
                }
            }
        }

        public void UnloadPlugin(string name)
        {
            Logger.LogDebug("[CSharpPluginLoader] Unloading " + name + " plugin.");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name)) {
                BasePlugin plugin = PluginLoader.GetInstance().Plugins[name];
                if (plugin.DontReload)
                    return;

                if (plugin.Globals.Contains("On_PluginDeinit"))
                    plugin.Invoke("On_PluginDeinit");

                plugin.KillTimers();
                PluginLoader.GetInstance().RemoveHooks(plugin);
                PluginLoader.GetInstance().Plugins.TryRemove(name, out plugin);

                Logger.LogDebug("[CSharpPluginLoader] " + name + " plugin was unloaded successfuly.");
            } else {
                Logger.LogError("[CSharpPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[CSharpPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
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
            LoadPlugins();
        }

        public bool CheckDependencies()
        {
            return CoreConfig.GetInstance().GetBoolValue("csharp", "enabled");
        }
    }
}

