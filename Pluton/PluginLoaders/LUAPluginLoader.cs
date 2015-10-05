using System;
using System.IO;

namespace Pluton
{
    public class LuaPluginLoader : Singleton<LuaPluginLoader>, ISingleton, IPluginLoader
    {
        public PluginType Type = PluginType.Lua;
        public readonly string Extension = ".lua";

        public LuaPluginLoader()
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
            Logger.LogDebug("[LUAPluginLoader] Loading plugin " + name + ".");

            if (PluginLoader.GetInstance().Plugins.ContainsKey(name))
            {
                Logger.LogError("[LUAPluginLoader] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[LUAPluginLoader] " + name + " plugin is already loaded.");
            }

            try
            {
                string code = GetSource(name);
                DirectoryInfo path = new DirectoryInfo(Path.Combine(PluginLoader.GetInstance().pluginDirectory.FullName, name));
                new LUAPlugin(name, code, path);

            }
            catch (Exception ex)
            {
                Logger.Log("[LUAPluginLoader] " + name + " plugin could not be loaded.");
                Logger.LogException(ex);
            }
        }

        public void LoadPlugins()
        {
            if (CoreConfig.GetInstance().GetBoolValue("lua", "enabled"))
            {
                foreach (string name in GetPluginNames())
                    LoadPlugin(name);
            }
            else
            {
                Logger.LogDebug("[LUAPluginLoader] Lua plugins are disabled in Core.cfg.");
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
            Logger.LogDebug("[LUAPluginLoader] Unloading " + name + " plugin.");

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

                Logger.LogDebug("[LUAPluginLoader] " + name + " plugin was unloaded successfuly.");
            }
            else
            {
                Logger.LogError("[LUAPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[LUAPluginLoader] Can't unload " + name + ". Plugin is not loaded.");
            }
        }

        public void UnloadPlugins()
        {
            foreach (string name in PluginLoader.GetInstance().Plugins.Keys)
                UnloadPlugin(name);
        }

        public void Initialize()
        {
            ReflectionExtensions.SetFieldValueValue(typeof(MoonSharp.Interpreter.Platforms.PlatformAutoDetector), "m_AutoDetectionsDone", true);
            ReflectionExtensions.SetFieldValueValue(typeof(MoonSharp.Interpreter.Platforms.PlatformAutoDetector), "<IsRunningOnUnity>k__BackingField", true);
            ReflectionExtensions.SetFieldValueValue(typeof(MoonSharp.Interpreter.Platforms.PlatformAutoDetector), "<IsRunningOnMono>k__BackingField", true);
            ReflectionExtensions.SetFieldValueValue(typeof(MoonSharp.Interpreter.Platforms.PlatformAutoDetector), "<IsRunningOnClr4>k__BackingField", true);
            PluginWatcher.GetInstance().AddWatcher(Type, Extension);
            PluginLoader.GetInstance().PluginLoaders.Add(Type, this);
            LoadPlugins();
        }

        public bool CheckDependencies()
        {
            return CoreConfig.GetInstance().GetBoolValue("lua", "enabled") &&
            File.Exists(Path.Combine(Path.Combine(Util.GetServerFolder(), "Managed"), "MoonSharp.Interpreter.dll"));
        }
    }
}

