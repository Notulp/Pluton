namespace Pluton {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;

	public class PluginLoader {

		private static Dictionary<string, Plugin> plugins;
		public static Dictionary<string, Plugin> Plugins { get { return plugins; } }
		private DirectoryInfo pluginDirectory;

		private static PluginLoader instance;

		public void Init() {
			pluginDirectory = new DirectoryInfo(Path.Combine(Util.GetPublicFolder(), "Plugins"));
			if (!Directory.Exists(pluginDirectory.FullName)) {
				Directory.CreateDirectory(pluginDirectory.FullName);
			}
			plugins = new Dictionary<string, Plugin>();
			ReloadPlugins();
			if (instance == null)
				instance = this;
		}

		public static PluginLoader GetInstance() {
			return (PluginLoader)instance;
		}

		private IEnumerable<String> GetPluginNames() {
			foreach (DirectoryInfo dirInfo in pluginDirectory.GetDirectories()) {
				string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".py");
				if (File.Exists(path)) yield return dirInfo.Name;
			}
		}

		private string GetPluginDirectoryPath(string name) {
			return Path.Combine(pluginDirectory.FullName, name);
		}

		private string GetPluginScriptPath(string name) {
			return Path.Combine(GetPluginDirectoryPath(name), name + ".py");
		}

		private string GetPluginScriptText(string name) {
			string path = GetPluginScriptPath(name);
			return File.ReadAllText(path);
		}

		#region re/un/loadplugin(s)

		public void LoadPlugins() {
			foreach (string name in GetPluginNames())
				LoadPlugin(name);

			//if(OnAllLoaded != null) OnAllLoaded();
		}

		public void UnloadPlugins() {
			foreach (string name in plugins.Keys)
				UnloadPlugin(name, false);
			plugins.Clear();
		}

		public void ReloadPlugins() {
			UnloadPlugins();
			LoadPlugins();
		}

		public void LoadPlugin(string name) {
			Logger.LogDebug("[Plugin] Loading plugin " + name + ".");

			if (plugins.ContainsKey(name)) {
				Logger.LogError("[Plugin] " + name + " plugin is already loaded.");
				throw new InvalidOperationException("[Plugin] " + name + " plugin is already loaded.");
			}

			try {
				string code = GetPluginScriptText(name);
				DirectoryInfo path = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
				Plugin plugin = new Plugin(name, code, path);

				InstallHooks(plugin);
				plugins.Add(name, plugin);

				Logger.Log("[Plugin] " + name + " plugin was loaded successfuly.");
			} catch (Exception ex) {
				Server.GetServer().Broadcast(name + " plugin could not be loaded.");
				Logger.LogException(ex);
			}
		}

		public void UnloadPlugin(string name, bool removeFromDict = true) {
			Logger.LogDebug("[Plugin] Unloading " + name + " plugin.");

			if (plugins.ContainsKey(name)) {
				Plugin plugin = plugins[name];

				plugin.KillTimers();
				RemoveHooks(plugin);
				if (removeFromDict) plugins.Remove(name);

				Logger.LogDebug("[Plugin] " + name + " plugin was unloaded successfuly.");
			} else {
				Logger.LogError("[Plugin] Can't unload " + name + ". Plugin is not loaded.");
				throw new InvalidOperationException("[Plugin] Can't unload " + name + ". Plugin is not loaded.");
			}
		}

		public void ReloadPlugin(Plugin plugin) {
			UnloadPlugin(plugin.Name);
			LoadPlugin(plugin.Name);
		}

		public void ReloadPlugin(string name) {
			UnloadPlugin(name);
			LoadPlugin(name);
		}

		#endregion

		#region install/remove hooks

		private void InstallHooks(Plugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Found function: " + method);
				//switch (method) { }
			}
		}

		private void RemoveHooks(Plugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Removing function: " + method);
				//switch (method) { }
			}
		}

		#endregion
	}
}

