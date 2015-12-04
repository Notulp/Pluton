namespace Pluton
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Reactive.Subjects;

    public class PluginLoader : Singleton<PluginLoader>, ISingleton
    {
        public ConcurrentDictionary<string, BasePlugin> Plugins = new ConcurrentDictionary<string, BasePlugin>();

        public DirectoryInfo pluginDirectory = new DirectoryInfo(Path.Combine(Util.GetPublicFolder(), "Plugins"));
        public Dictionary<PluginType, IPluginLoader> PluginLoaders = new Dictionary<PluginType, IPluginLoader>();

        public List<String> CurrentlyLoadingPlugins = new List<string>();

        public Subject<string> OnAllLoaded = new Subject<string>();

        public void Initialize()
        {
            PYPlugin.LibPath = Path.Combine(Util.GetPublicFolder(), Path.Combine("Python", "Lib"));
            BasePlugin.GlobalData = new Dictionary<string, object>();
            pluginDirectory = new DirectoryInfo(Path.Combine(Util.GetPublicFolder(), "Plugins"));
            if (!Directory.Exists(pluginDirectory.FullName)) {
                Directory.CreateDirectory(pluginDirectory.FullName);
            }
        }

        public bool CheckDependencies()
        {
            return true;
        }

        #region re/un/loadplugin(s)

        public void OnPluginLoaded(BasePlugin plugin)
        {
            if (PluginLoader.GetInstance().CurrentlyLoadingPlugins.Contains(plugin.Name)) {
                PluginLoader.GetInstance().CurrentlyLoadingPlugins.Remove(plugin.Name);
            }

            if (plugin.State != PluginState.Loaded) {
                throw new FileLoadException("Couldn't initialize " + plugin.Type.ToString() + " plugin.", 
                    Path.Combine(Path.Combine(pluginDirectory.FullName, plugin.Name), plugin.Name + plugin.Type.ToString())
                );
            }

            InstallHooks(plugin);
            Plugins.TryAdd(plugin.Name, plugin);

            // probably make an event here that others can hook?

            if (CurrentlyLoadingPlugins.Count == 0)
                Hooks.OnNext("On_AllPluginLoaded");

            Logger.Log(String.Format("[PluginLoader] {0}<{1}> plugin was loaded successfuly.", plugin.Name, plugin.Type));
        }

        public void LoadPlugin(string name, PluginType t)
        {
            PluginLoaders[t].LoadPlugin(name);
        }

        public void LoadPlugins()
        {
            foreach (IPluginLoader loader in PluginLoaders.Values) {
                loader.LoadPlugins();
            }
        }

        public void UnloadPlugins()
        {
            foreach (IPluginLoader loader in PluginLoaders.Values) {
                loader.UnloadPlugins();
            }
        }

        public void ReloadPlugins()
        {
            foreach (IPluginLoader loader in PluginLoaders.Values) {
                loader.ReloadPlugins();
            }
        }

        public void ReloadPlugin(string name)
        {
            if (Plugins.ContainsKey(name)) {
                PluginLoaders[Plugins[name].Type].ReloadPlugin(name);
            }
        }

        public void ReloadPlugin(BasePlugin plugin)
        {
            if (Plugins.ContainsKey(plugin.Name)) {
                var loader = PluginLoaders[plugin.Type];
                string name = plugin.Name;
                loader.UnloadPlugin(name);
                plugin = null;
                Plugins.TryRemove(name, out plugin);
                loader.LoadPlugin(name);
            }
        }

        #endregion

        #region install/remove hooks

        public void InstallHooks(BasePlugin plugin)
        {
            if (plugin.State != PluginState.Loaded)
                return;

            foreach (string method in plugin.Globals) {
                if (Hooks.HookNames.Contains(method)) {
                    plugin.Hooks.Add(
                        Hooks.Subscribe(method, plugin)
                    );
                    Logger.LogDebug($"[{plugin.Type}] Adding hook: {plugin.Name}.{method}");
                }
            }

            if (plugin.Globals.Contains("On_PluginInit"))
                plugin.Invoke("On_PluginInit");
        }

        public void RemoveHooks(BasePlugin plugin)
        {
            if (plugin.State != PluginState.Loaded)
                return;

            foreach (Hook hook in plugin.Hooks) {
                Logger.LogDebug($"[{plugin.Type}] Removing hook: {plugin.Name}.{hook.Name}");
                hook.hook.Dispose();
            }
            plugin.Hooks.Clear();

            if (plugin.Globals.Contains("On_PluginDeinit"))
                plugin.Invoke("On_PluginDeinit");
        }

        #endregion
    }
}

