namespace Pluton
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reactive.Subjects;
    using System.Reflection;

    public class PluginLoader
    {

        private static Dictionary<string, Plugin> plugins;

        public static Dictionary<string, Plugin> Plugins { get { return plugins; } }

        private DirectoryInfo pluginDirectory;
        private static PluginLoader instance;

        private static Subject<string> OnLoadCommands = new Subject<string>();

        public static void LoadCommands()
        {
            OnLoadCommands.OnNext("");
        }

        public void Init()
        {
            Plugin.LibPath = Path.Combine(Util.GetPublicFolder(), Path.Combine("Python", "Lib"));
            pluginDirectory = new DirectoryInfo(Path.Combine(Util.GetPublicFolder(), "Plugins"));
            if (!Directory.Exists(pluginDirectory.FullName)) {
                Directory.CreateDirectory(pluginDirectory.FullName);
            }
            plugins = new Dictionary<string, Plugin>();
            ReloadPlugins();
        }

        public static PluginLoader GetInstance()
        {
            if (instance == null)
                instance = new PluginLoader();
            return instance;
        }

        private IEnumerable<String> GetJSPluginNames()
        {
            foreach (DirectoryInfo dirInfo in pluginDirectory.GetDirectories()) {
                string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".js");
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
        }

        private IEnumerable<String> GetPyPluginNames()
        {
            foreach (DirectoryInfo dirInfo in pluginDirectory.GetDirectories()) {
                string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".py");
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
        }

        private string GetPluginDirectoryPath(string name)
        {
            return Path.Combine(pluginDirectory.FullName, name);
        }

        private string GetJSPluginScriptPath(string name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".js");
        }

        private string GetPyPluginScriptPath(string name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".py");
        }

        private string GetPluginScriptText(string name, Plugin.PluginType type)
        {
            string path = "";
            if (type == Plugin.PluginType.Python)
                path = GetPyPluginScriptPath(name);
            else
                path = GetJSPluginScriptPath(name);

            if (path == "") return null;

            return File.ReadAllText(path);
        }

        #region re/un/loadplugin(s)

        public void LoadPlugins()
        {
            foreach (string name in GetPyPluginNames())
                LoadPlugin(name, Plugin.PluginType.Python);

            //foreach (string name in GetJSPluginNames())
            //   LoadPlugin(name, Plugin.PluginType.JS);
            //if(OnAllLoaded != null) OnAllLoaded();
        }

        public void UnloadPlugins()
        {
            foreach (string name in plugins.Keys)
                UnloadPlugin(name, false);
            plugins.Clear();
        }

        public void ReloadPlugins()
        {
            UnloadPlugins();
            LoadPlugins();
        }

        public void LoadPlugin(string name, Plugin.PluginType type)
        {
            Logger.LogDebug("[PluginLoader] Loading plugin " + name + ".");

            if (plugins.ContainsKey(name)) {
                Logger.LogError("[PluginLoader] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[PluginLoader] " + name + " plugin is already loaded.");
            }

            try {
                string code = GetPluginScriptText(name, type);
                DirectoryInfo path = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                Plugin plugin = new Plugin(name, code, path, type);

                InstallHooks(plugin);
                plugins.Add(name, plugin);

                Logger.Log("[PluginLoader] " + name + " plugin was loaded successfuly.");
            } catch (Exception ex) {
                Server.GetServer().Broadcast(name + " plugin could not be loaded.");
                Logger.LogException(ex);
            }
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Logger.LogDebug("[PluginLoader] Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name)) {
                Plugin plugin = plugins[name];

                plugin.KillTimers();
                RemoveHooks(plugin);
                if (removeFromDict)
                    plugins.Remove(name);

                Logger.LogDebug("[PluginLoader] " + name + " plugin was unloaded successfuly.");
            } else {
                Logger.LogError("[PluginLoader] Can't unload " + name + ". Plugin is not loaded.");
                throw new InvalidOperationException("[PluginLoader] Can't unload " + name + ". Plugin is not loaded.");
            }
        }

        public void ReloadPlugin(Plugin plugin)
        {
            UnloadPlugin(plugin.Name);
            LoadPlugin(plugin.Name, plugin.Type);
        }

        public void ReloadPlugin(string name)
        {
            if (plugins.ContainsKey(name)) {
                Plugin plugin = plugins[name];
                UnloadPlugin(plugin.Name);
                LoadPlugin(plugin.Name, plugin.Type);
            }
        }

        #endregion

        #region install/remove hooks

        private void InstallHooks(Plugin plugin)
        {
            foreach (string method in plugin.Globals) {
                if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
                    continue;

                Logger.LogDebug("Found function: " + method);
                switch (method) {
                case "On_Chat":
                    plugin.OnChatHook = Hooks.OnChat.Subscribe(c => plugin.OnChat(c));
                    break;
                case "On_ClientAuth":
                    plugin.OnClientAuthHook = Hooks.OnClientAuth.Subscribe(a => plugin.OnClientAuth(a));
                    break;
                case "On_Command":
                    plugin.OnCommandHook = Hooks.OnCommand.Subscribe(c => plugin.OnCommand(c));
                    break;
                case "On_CorpseDropped":
                    plugin.OnCorpseDroppedHook = Hooks.OnCorpseDropped.Subscribe(c => plugin.OnCorpseDropped(c));
                    break;
                case "On_CorpseAttacked":
                    plugin.OnCorpseAttackedHook = Hooks.OnCorpseAttacked.Subscribe(c => plugin.OnCorpseAttacked(c));
                    break;
                case "On_BuildingComplete":
                    plugin.OnBuildingCompleteHook = Hooks.OnBuildingComplete.Subscribe(b => plugin.OnBuildingComplete(b));
                    break;
                case "On_BuildingUpdate":
                    plugin.OnBuildingUpdateHook = Hooks.OnBuildingUpdate.Subscribe(b => plugin.OnBuildingUpdate(b));
                    break;
                case "On_BuildingPartAttacked":
                    plugin.OnBuildingPartAttackedHook = Hooks.OnBuildingPartAttacked.Subscribe(b => plugin.OnBuildingPartAttacked(b));
                    break;
                case "On_BuildingPartDestroyed":
                    plugin.OnBuildingPartDestroyedHook = Hooks.OnBuildingPartDestroyed.Subscribe(b => plugin.OnBuildingPartDestroyed(b));
                    break;
                case "On_FrameDeployed":
                    plugin.OnFrameDeployedHook = Hooks.OnBuildingFrameDeployed.Subscribe(f => plugin.OnFrameDeployed(f));
                    break;
                case "On_NPCAttacked":
                    plugin.OnNPCHurtHook = Hooks.OnNPCHurt.Subscribe(n => plugin.OnNPCHurt(n));
                    break;
                case "On_NPCKilled":
                    plugin.OnNPCKilledHook = Hooks.OnNPCDied.Subscribe(n => plugin.OnNPCKilled(n));
                    break;
                case "On_LootingEntity":
                    plugin.OnLootingEntityHook = Hooks.OnLootingEntity.Subscribe(l => plugin.OnLootingEntity(l));
                    break;
                case "On_LootingPlayer":
                    plugin.OnLootingPlayerHook = Hooks.OnLootingPlayer.Subscribe(l => plugin.OnLootingPlayer(l));
                    break;
                case "On_LootingItem":
                    plugin.OnLootingItemHook = Hooks.OnLootingItem.Subscribe(l => plugin.OnLootingItem(l));
                    break;
                case "On_PlayerConnected":
                    plugin.OnPlayerConnectedHook = Hooks.OnPlayerConnected.Subscribe(p => plugin.OnPlayerConnected(p));
                    break;
                case "On_PlayerDisconnected":
                    plugin.OnPlayerDisconnectedHook = Hooks.OnPlayerDisconnected.Subscribe(p => plugin.OnPlayerDisconnected(p));
                    break;
                case "On_PlayerGathering":
                    plugin.OnPlayerGatheringHook = Hooks.OnGathering.Subscribe(g => plugin.OnPlayerGathering(g));
                    break;
                case "On_PlayerAttacked":
                    plugin.OnPlayerAttackedHook = Hooks.OnPlayerHurt.Subscribe(p => plugin.OnPlayerAttacked(p));
                    break;
                case "On_PlayerDied":
                    plugin.OnPlayerDiedHook = Hooks.OnPlayerDied.Subscribe(p => plugin.OnPlayerDied(p));
                    break;
                case "On_PlayerTakeDamage":
                    plugin.OnPlayerTakeDamageHook = Hooks.OnPlayerTakeDamage.Subscribe(p => plugin.OnPlayerTakeDamage(p));
                    break;
                case "On_PlayerTakeRadiation":
                    plugin.OnPlayerTakeRadiationHook = Hooks.OnPlayerTakeRads.Subscribe(p => plugin.OnPlayerTakeRadiation(p));
                    break;
                case "On_ServerShutdown":
                    plugin.OnServerShutdownHook = Hooks.OnServerShutdown.Subscribe(s => plugin.OnServerShutdown(s));
                    break;
                case "On_Respawn":
                    plugin.OnRespawnHook = Hooks.OnRespawn.Subscribe(r => plugin.OnRespawn(r));
                    break;
                case "On_LoadingCommands":
                    plugin.OnLoadCommandsHook = OnLoadCommands.Subscribe(n => plugin.OnLoadCommands(n));
                    break;
                case "On_PluginInit":
                    plugin.Invoke("On_PluginInit");
                    break;
                }
            }
        }

        private void RemoveHooks(Plugin plugin)
        {
            foreach (string method in plugin.Globals) {
                if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
                    continue;

                Logger.LogDebug("Removing function: " + method);
                switch (method) {
                case "On_Chat":
                    plugin.OnChatHook.Dispose();
                    break;
                case "On_ClientAuth":
                    plugin.OnClientAuthHook.Dispose();
                    break;
                case "On_Command":
                    plugin.OnCommandHook.Dispose();
                    break;
                case "On_CorpseDropped":
                    plugin.OnCorpseDroppedHook.Dispose();
                    break;
                case "On_CorpseAttacked":
                    plugin.OnCorpseAttackedHook.Dispose();
                    break;
                case "On_BuildingComplete":
                    plugin.OnBuildingCompleteHook.Dispose();
                    break;
                case "On_BuildingUpdate":
                    plugin.OnBuildingUpdateHook.Dispose();
                    break;
                case "On_BuildingPartAttacked":
                    plugin.OnBuildingPartAttackedHook.Dispose();
                    break;
                case "On_BuildingPartDestroyed":
                    plugin.OnBuildingPartDestroyedHook.Dispose();
                    break;
                case "On_FrameDeployed":
                    plugin.OnFrameDeployedHook.Dispose();
                    break;
                case "On_NPCAttacked":
                    plugin.OnNPCHurtHook.Dispose();
                    break;
                case "On_NPCKilled":
                    plugin.OnNPCKilledHook.Dispose();
                    break;
                case "On_LootingEntity":
                    plugin.OnLootingEntityHook.Dispose();
                    break;
                case "On_LootingPlayer":
                    plugin.OnLootingPlayerHook.Dispose();
                    break;
                case "On_LootingItem":
                    plugin.OnLootingItemHook.Dispose();
                    break;
                case "On_PlayerConnected":
                    plugin.OnPlayerConnectedHook.Dispose();
                    break;
                case "On_PlayerDisconnected":
                    plugin.OnPlayerDisconnectedHook.Dispose();
                    break;
                case "On_PlayerGathering":
                    plugin.OnPlayerGatheringHook.Dispose();
                    break;
                case "On_PlayerAttacked":
                    plugin.OnPlayerAttackedHook.Dispose();
                    break;
                case "On_PlayerDied":
                    plugin.OnPlayerDiedHook.Dispose();
                    break;
                case "On_PlayerTakeDamage":
                    plugin.OnPlayerTakeDamageHook.Dispose();
                    break;
                case "On_PlayerTakeRadiation":
                    plugin.OnPlayerTakeRadiationHook.Dispose();
                    break;
                case "On_ServerShutdown":
                    plugin.OnServerShutdownHook.Dispose();
                    break;
                case "On_Respawn":
                    plugin.OnRespawnHook.Dispose();
                    break;
                case "On_LoadingCommands":
                    plugin.OnLoadCommandsHook.Dispose();
                    break;
                }
            }
        }

        #endregion
    }
}

