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
		}

		public static PluginLoader GetInstance() {
			if (instance == null)
				instance = new PluginLoader();
			return instance;
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
				switch(method) {
				case "On_Chat":
					Hooks.OnChat += new Hooks.ChatDelegate(plugin.OnChat);
					break;
				case "On_ClientAuth":
					Hooks.OnClientAuth += new Hooks.ClientAuthDelegate(plugin.OnClientAuth);
					break;
				case "On_Command":
					Hooks.OnCommand += new Hooks.CommandDelegate(plugin.OnCommand);
					break;
				case "On_CorpseDropped":
					Hooks.OnCorpseDropped += new Hooks.CorpseDropDelegate(plugin.OnCorpseDropped);
					break;
				case "On_CorpseAttacked":
					Hooks.OnCorpseAttacked += new Hooks.CorpseAttackedDelegate(plugin.OnCorpseAttacked);
					break;
				case "On_BuildingComplete":
					Hooks.OnBuildingComplete += new Hooks.BuildingCompleteDelegate(plugin.OnBuildingComplete);
					break;
				case "On_BuildingUpdate":
					Hooks.OnBuildingUpdate += new Hooks.BuildingUpdateDelegate(plugin.OnBuildingUpdate);
					break;
				case "On_BuildingPartAttacked":
					Hooks.OnBuildingPartAttacked += new Hooks.BuildingPartAttackedDelegate(plugin.OnBuildingPartAttacked);
					break;
				case "On_BuildingPartDestroyed":
					Hooks.OnBuildingPartDestroyed += new Hooks.BuildingPartDestroyedDelegate(plugin.OnBuildingPartDestroyed);
					break;
				case "On_FrameDeployed":
					Hooks.OnBuildingFrameDeployed += new Hooks.BuildingFrameDeployedDelegate(plugin.OnFrameDeployed);
					break;
				case "On_NPCAttacked":
					Hooks.OnNPCHurt += new Hooks.NPCHurtDelegate(plugin.OnNPCHurt);
					break;
				case "On_NPCKilled":
					Hooks.OnNPCDied += new Hooks.NPCDiedDelegate(plugin.OnNPCKilled);
					break;
				case "On_LootingEntity":
					Hooks.OnLootingEntity += new Hooks.LootingEntityDelegate(plugin.OnLootingEntity);
					break;
				case "On_LootingPlayer":
					Hooks.OnLootingPlayer += new Hooks.LootingPlayerDelegate(plugin.OnLootingPlayer);
					break;
				case "On_LootingItem":
					Hooks.OnLootingItem += new Hooks.LootingItemDelegate(plugin.OnLootingItem);
					break;
				case "On_PlayerConnected":
					Hooks.OnPlayerConnected += new Hooks.PlayerConnectedDelegate(plugin.OnPlayerConnected);
					break;
				case "On_PlayerDisconnected":
					Hooks.OnPlayerDisconnected += new Hooks.PlayerDisconnectedDelegate(plugin.OnPlayerDisconnected);
					break;
				case "On_PlayerGathering":
					Hooks.OnGathering += new Hooks.GatheringDelegate(plugin.OnPlayerGathering);
					break;
				case "On_PlayerAttacked":
					Hooks.OnPlayerHurt += new Hooks.PlayerHurtDelegate(plugin.OnPlayerAttacked);
					break;
				case "On_PlayerDied":
					Hooks.OnPlayerDied += new Hooks.PlayerDiedDelegate(plugin.OnPlayerDied);
					break;
				case "On_PlayerTakeDamage":
					Hooks.OnPlayerTakeDamage += new Hooks.PlayerTakeDamageDelegate(plugin.OnPlayerTakeDamage);
					break;
				case "On_PlayerTakeRadiation":
					Hooks.OnPlayerTakeRads += new Hooks.PlayerTakeRadsDelegate(plugin.OnPlayerTakeRadiation);
					break;
				case "On_ServerShutdown":
					Hooks.OnServerShutdown += new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown);
					break;
				case "On_PluginInit":
					plugin.Invoke("On_PluginInit");
					break;
				}
			}
		}

		private void RemoveHooks(Plugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Removing function: " + method);
				switch (method) {
				case "On_Chat":
					Hooks.OnChat -= new Hooks.ChatDelegate(plugin.OnChat);
					break;
				case "On_ClientAuth":
					Hooks.OnClientAuth -= new Hooks.ClientAuthDelegate(plugin.OnClientAuth);
					break;
				case "On_Command":
					Hooks.OnCommand -= new Hooks.CommandDelegate(plugin.OnCommand);
					break;
				case "On_CorpseDropped":
					Hooks.OnCorpseDropped -= new Hooks.CorpseDropDelegate(plugin.OnCorpseDropped);
					break;
				case "On_CorpseAttacked":
					Hooks.OnCorpseAttacked -= new Hooks.CorpseAttackedDelegate(plugin.OnCorpseAttacked);
					break;
				case "On_BuildingComplete":
					Hooks.OnBuildingComplete -= new Hooks.BuildingCompleteDelegate(plugin.OnBuildingComplete);
					break;
				case "On_BuildingUpdate":
					Hooks.OnBuildingUpdate -= new Hooks.BuildingUpdateDelegate(plugin.OnBuildingUpdate);
					break;
				case "On_BuildingPartAttacked":
					Hooks.OnBuildingPartAttacked -= new Hooks.BuildingPartAttackedDelegate(plugin.OnBuildingPartAttacked);
					break;
				case "On_BuildingPartDestroyed":
					Hooks.OnBuildingPartDestroyed -= new Hooks.BuildingPartDestroyedDelegate(plugin.OnBuildingPartDestroyed);
					break;
				case "On_FrameDeployed":
					Hooks.OnBuildingFrameDeployed -= new Hooks.BuildingFrameDeployedDelegate(plugin.OnFrameDeployed);
					break;
				case "On_NPCAttacked":
					Hooks.OnNPCHurt -= new Hooks.NPCHurtDelegate(plugin.OnNPCHurt);
					break;
				case "On_NPCKilled":
					Hooks.OnNPCDied -= new Hooks.NPCDiedDelegate(plugin.OnNPCKilled);
					break;
				case "On_LootingEntity":
					Hooks.OnLootingEntity -= new Hooks.LootingEntityDelegate(plugin.OnLootingEntity);
					break;
				case "On_LootingPlayer":
					Hooks.OnLootingPlayer -= new Hooks.LootingPlayerDelegate(plugin.OnLootingPlayer);
					break;
				case "On_LootingItem":
					Hooks.OnLootingItem -= new Hooks.LootingItemDelegate(plugin.OnLootingItem);
					break;
				case "On_PlayerConnected":
					Hooks.OnPlayerConnected -= new Hooks.PlayerConnectedDelegate(plugin.OnPlayerConnected);
					break;
				case "On_PlayerDisconnected":
					Hooks.OnPlayerDisconnected -= new Hooks.PlayerDisconnectedDelegate(plugin.OnPlayerDisconnected);
					break;
				case "On_PlayerGathering":
					Hooks.OnGathering -= new Hooks.GatheringDelegate(plugin.OnPlayerGathering);
					break;
				case "On_PlayerAttacked":
					Hooks.OnPlayerHurt -= new Hooks.PlayerHurtDelegate(plugin.OnPlayerAttacked);
					break;
				case "On_PlayerDied":
					Hooks.OnPlayerDied -= new Hooks.PlayerDiedDelegate(plugin.OnPlayerDied);
					break;
				case "On_PlayerTakeDamage":
					Hooks.OnPlayerTakeDamage -= new Hooks.PlayerTakeDamageDelegate(plugin.OnPlayerTakeDamage);
					break;
				case "On_PlayerTakeRadiation":
					Hooks.OnPlayerTakeRads -= new Hooks.PlayerTakeRadsDelegate(plugin.OnPlayerTakeRadiation);
					break;
				case "On_ServerShutdown":
					Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown);
					break;
				}
			}
		}

		#endregion
	}
}

