using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Pluton
{
    public class PluginWatcher : Singleton<PluginWatcher>, ISingleton
    {
        public List<PluginTypeWatcher> Watchers = new List<PluginTypeWatcher>();

        public PluginWatcher() {}

        public void AddWatcher(PluginType type, string filter)
        {
            foreach (PluginTypeWatcher watch in Watchers)
                if (watch.Type == type) return;

            PluginTypeWatcher watcher = new PluginTypeWatcher(type, filter);
            Watchers.Add(watcher);
        }

        public void Initialize()
        {
        }
    }

    public class PluginTypeWatcher : CountedInstance
    {
        public PluginType Type;

        public FileSystemWatcher Watcher;

        public PluginTypeWatcher(PluginType type, string filter)
        {
            Type = type;
            Watcher = new FileSystemWatcher(Path.Combine(Util.GetPublicFolder(), "Plugins"), "*" + filter);
            Watcher.EnableRaisingEvents = true;
            Watcher.IncludeSubdirectories = true;
            Watcher.Changed += OnPluginChanged;
            Watcher.Created += OnPluginCreated;

        }

        public override string ToString()
        {
            return String.Format("PluginTypeWatcher<{0}>", Type);
        }

        bool TryLoadPlugin(string name, PluginType type)
        {
            try {
                BasePlugin plugin = null;
                if (PluginLoader.GetInstance().Plugins.TryGetValue(name, out plugin))
                    PluginLoader.GetInstance().ReloadPlugin(plugin);
                else
                    PluginLoader.GetInstance().LoadPlugin(name, type);

                return true;

            } catch (Exception ex) {
                Pluton.Logger.LogException(ex);
                return false;
            }
        }

        void OnPluginCreated (object sender, FileSystemEventArgs e)
        {
            string filename = Path.GetFileNameWithoutExtension(e.Name);
            string dir = Path.GetDirectoryName(e.FullPath).Split(Path.DirectorySeparatorChar).Last();

            if (filename == dir) {
                if (!TryLoadPlugin(filename, Type)) {
                    Pluton.Logger.Log(String.Format("[PluginWatcher] Couldn't load: {0}{3}{1}.{2}", dir, filename, Type, Path.DirectorySeparatorChar));
                }
            }
        }

        void OnPluginChanged (object sender, FileSystemEventArgs e)
        {
            string filename = Path.GetFileNameWithoutExtension(e.Name);
            string dir = Path.GetDirectoryName(e.FullPath).Split(Path.DirectorySeparatorChar).Last();

            string assumedPluginPathFromDir = Path.Combine(Path.Combine(Watcher.Path, dir), dir + Path.GetExtension(e.Name));

            if (filename == dir) {
                if (File.Exists(e.FullPath)) {
                    if (!TryLoadPlugin(filename, Type)) {
                        Pluton.Logger.Log(String.Format("[PluginWatcher] Couldn't load: {0}{3}{1}.{2}", dir, filename, Type, Path.DirectorySeparatorChar));
                    }
                }
            } else if (File.Exists(assumedPluginPathFromDir)) {
                if (!TryLoadPlugin(dir, Type)) {
                    Pluton.Logger.Log(String.Format("[PluginWatcher] Couldn't load: {0}{3}{1}.{2}", dir, filename, Type, Path.DirectorySeparatorChar));
                }
            }
        }
    }
}

