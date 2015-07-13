using System.Collections.Generic;

namespace Pluton
{
    public interface IPluginLoader
    {
        string GetExtension();

        string GetSource(string path);

        string GetMainFilePath(string pluginname);

        string GetPluginDirectoryPath(string name);

        IEnumerable<string> GetPluginNames();

        void LoadPlugin(string name);

        void LoadPlugins();

        void ReloadPlugin(string name);

        void ReloadPlugins();

        void UnloadPlugin(string name);

        void UnloadPlugins();
    }
}

