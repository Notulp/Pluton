namespace Pluton
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reactive.Subjects;
    using System.Reflection;

    public class PluginLoader : Singleton<PluginLoader>, ISingleton
    {

        private Dictionary<string, BasePlugin> plugins = new Dictionary<string, BasePlugin>();

        public Dictionary<string, BasePlugin> Plugins { get { return plugins; } }

        private DirectoryInfo pluginDirectory;

        public Subject<string> OnAllLoaded = new Subject<string>();

        public void Initialize()
        {
            PYPlugin.LibPath = Path.Combine(Util.GetPublicFolder(), Path.Combine("Python", "Lib"));
            BasePlugin.GlobalData = new Dictionary<string, object>();
            pluginDirectory = new DirectoryInfo(Path.Combine(Util.GetPublicFolder(), "Plugins"));
            if (!Directory.Exists(pluginDirectory.FullName)) {
                Directory.CreateDirectory(pluginDirectory.FullName);
            }
            ReloadPlugins();
        }

        private IEnumerable<String> GetCSharpPluginNames()
        {
            foreach (DirectoryInfo dirInfo in pluginDirectory.GetDirectories()) {
                string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".dll");
                if (File.Exists(path))
                    yield return dirInfo.Name;
            }
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

        private string GetCSharpPluginScriptPath(string name)
        {
            return Path.Combine(GetPluginDirectoryPath(name), name + ".dll");
        }

        private string GetPluginScriptText(string name, PluginType type)
        {
            string path = "";
            if (type == PluginType.Python)
                path = GetPyPluginScriptPath(name);
            else if (type == PluginType.JavaScript)
                path = GetJSPluginScriptPath(name);
            else if (type == PluginType.CSharp)
                return GetCSharpPluginScriptPath(name);

            if (path == "") return null;

            return File.ReadAllText(path);
        }

        #region re/un/loadplugin(s)

        public void LoadPlugins()
        {
            if (CoreConfig.GetInstance().GetBoolValue("python", "enabled")) {
                PluginWatcher.GetInstance().AddWatcher(PluginType.Python, "*.py");
                foreach (string name in GetPyPluginNames())
                    LoadPlugin(name, PluginType.Python);
            } else
                Logger.LogDebug("[PluginLoader] Python plugins are disabled in Core.cfg.");

            if (CoreConfig.GetInstance().GetBoolValue("javascript", "enabled")) {
                PluginWatcher.GetInstance().AddWatcher(PluginType.JavaScript, "*.js");
                foreach (string name in GetJSPluginNames())
                    LoadPlugin(name, PluginType.JavaScript);
            } else
                Logger.LogDebug("[PluginLoader] Javascript plugins are disabled in Core.cfg.");

            if (CoreConfig.GetInstance().GetBoolValue("csharp", "enabled")) {
                PluginWatcher.GetInstance().AddWatcher(PluginType.CSharp, "*.dll");
                foreach (string name in GetCSharpPluginNames())
                    LoadPlugin(name, PluginType.CSharp);
            } else
                Logger.LogDebug("[PluginLoader] CSharp plugins are disabled in Core.cfg.");

            OnAllLoaded.OnNext("");
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

        public void LoadPlugin(string name, PluginType type)
        {
            Logger.LogDebug("[PluginLoader] Loading plugin " + name + ".");

            if (plugins.ContainsKey(name)) {
                Logger.LogError("[PluginLoader] " + name + " plugin is already loaded.");
                throw new InvalidOperationException("[PluginLoader] " + name + " plugin is already loaded.");
            }

            try {
                string code = GetPluginScriptText(name, type);
                DirectoryInfo path = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
                BasePlugin plugin = null;
                switch(type){
                case PluginType.CSharp:
                    plugin = new CSPlugin(name, code, path);
                    break;
                case PluginType.JavaScript:
                    plugin = new JSPlugin(name, code, path);
                    break;
                case PluginType.Python:
                    plugin = new PYPlugin(name, code, path);
                    break;
                }

                InstallHooks(plugin);
                plugins.Add(name, plugin);

                Logger.Log("[PluginLoader] " + name + " plugin was loaded successfuly.");
            } catch (Exception ex) {
                Server.GetInstance().Broadcast(name + " plugin could not be loaded.");
                Logger.Log("[PluginLoader] " + name + " plugin could not be loaded.");
                Logger.LogException(ex);
            }
        }

        public void UnloadPlugin(string name, bool removeFromDict = true)
        {
            Logger.LogDebug("[PluginLoader] Unloading " + name + " plugin.");

            if (plugins.ContainsKey(name)) {
                BasePlugin plugin = plugins[name];

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

        public void ReloadPlugin(BasePlugin plugin)
        {
            UnloadPlugin(plugin.Name);
            LoadPlugin(plugin.Name, plugin.Type);

        }

        public void ReloadPlugin(string name)
        {
            if (plugins.ContainsKey(name)) {
                BasePlugin plugin = plugins[name];
                UnloadPlugin(plugin.Name);
                LoadPlugin(plugin.Name, plugin.Type);
            }
        }

        #endregion

        #region install/remove hooks

        private void InstallHooks(BasePlugin plugin)
        {
            if (plugin.State != PluginState.Loaded)
                return;

            foreach (string method in plugin.Globals) {
                if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
                    continue;

                bool foundHook = true;
                switch (method) {
                case "On_AllPluginsLoaded":
                    plugin.OnAllPluginsLoadedHook = OnAllLoaded.Subscribe(s => plugin.OnAllPluginsLoaded(""));
                    break;
                case "On_Chat":
                    plugin.OnChatHook = Hooks.OnChat.Subscribe(c => plugin.OnChat(c));
                    break;
                case "On_ClientAuth":
                    plugin.OnClientAuthHook = Hooks.OnClientAuth.Subscribe(a => plugin.OnClientAuth(a));
                    break;
                case "On_ClientConsole":
                    plugin.OnClientConsoleHook = Hooks.OnClientConsole.Subscribe(c => plugin.OnClientConsole(c));
                    break;
                case "On_Command":
                    plugin.OnCommandHook = Hooks.OnCommand.Subscribe(c => plugin.OnCommand(c));
                    break;
                case "On_CommandPermission":
                    plugin.OnCommandPermissionHook = Hooks.OnCommandPermission.Subscribe(c => plugin.OnCommandPermission(c));
                    break;
                case "On_CorpseHurt":
                    plugin.OnCorpseHurtHook = Hooks.OnCorpseHurt.Subscribe(c => plugin.OnCorpseHurt(c));
                    break;
                case "On_BuildingComplete":
                    plugin.OnBuildingCompleteHook = Hooks.OnBuildingComplete.Subscribe(b => plugin.OnBuildingComplete(b));
                    break;
                case "On_Placement":
                    plugin.OnPlacementHook = Hooks.OnPlacement.Subscribe(b => plugin.OnPlacement(b));
                    break;
                case "On_DoorCode":
                    plugin.OnDoorCodeHook = Hooks.OnDoorCode.Subscribe(b => plugin.OnDoorCode(b));
                    break;
                case "On_DoorUse":
                    plugin.OnDoorUseHook = Hooks.OnDoorUse.Subscribe(d => plugin.OnDoorUse(d));
                    break;
                case "On_NPCHurt":
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
                case "On_PlayerHurt":
                    plugin.OnPlayerHurtHook = Hooks.OnPlayerHurt.Subscribe(p => plugin.OnPlayerHurt(p));
                    break;
                case "On_CombatEntityHurt":
                    plugin.OnCombatEntityHurtHook = Hooks.OnCombatEntityHurt.Subscribe(c => plugin.OnCombatEntityHurt(c));
                    break;
                case "On_PlayerDied":
                    plugin.OnPlayerDiedHook = Hooks.OnPlayerDied.Subscribe(p => plugin.OnPlayerDied(p));
                    break;
                case "On_PlayerStartCrafting":
                    plugin.OnPlayerStartCraftingHook = Hooks.OnPlayerStartCrafting.Subscribe(p => plugin.OnPlayerStartCrafting(p));
                    break;
                case "On_PlayerTakeRadiation":
                    plugin.OnPlayerTakeRadiationHook = Hooks.OnPlayerTakeRads.Subscribe(p => plugin.OnPlayerTakeRadiation(p));
                    break;
                case "On_ServerConsole":
                    plugin.OnServerConsoleHook = Hooks.OnServerConsole.Subscribe(c => plugin.OnServerConsole(c));
                    break;
                case "On_ServerInit":
                    plugin.OnServerInitHook = Hooks.OnServerInit.Subscribe(s => plugin.OnServerInit(s));
                    break;
                case "On_ServerShutdown":
                    plugin.OnServerShutdownHook = Hooks.OnServerShutdown.Subscribe(s => plugin.OnServerShutdown(s));
                    break;
                case "On_Respawn":
                    plugin.OnRespawnHook = Hooks.OnRespawn.Subscribe(r => plugin.OnRespawn(r));
                    break;
                case "On_PluginInit":
                    plugin.Invoke("On_PluginInit");
                    break;
                default:
                    foundHook = false;
                    break;
                }
                if(foundHook)
                    Logger.LogDebug("Found hook: " + method);
            }
        }

        private void RemoveHooks(BasePlugin plugin)
        {
            if (plugin.State != PluginState.Loaded)
                return;

            foreach (string method in plugin.Globals) {
                if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
                    continue;

                bool foundHook = true;
                switch (method) {
                case "On_AllPluginsLoaded":
                    plugin.OnAllPluginsLoadedHook.Dispose();
                    break;
                case "On_Chat":
                    plugin.OnChatHook.Dispose();
                    break;
                case "On_ClientAuth":
                    plugin.OnClientAuthHook.Dispose();
                    break;
                case "On_ClientConsole":
                    plugin.OnClientConsoleHook.Dispose();
                    break;
                case "On_Command":
                    plugin.OnCommandHook.Dispose();
                    break;
                case "On_CommandPermission":
                    plugin.OnCommandPermissionHook.Dispose();
                    break;
                case "On_CorpseHurt":
                    plugin.OnCorpseHurtHook.Dispose();
                    break;
                case "On_BuildingComplete":
                    plugin.OnBuildingCompleteHook.Dispose();
                    break;
                case "On_Placement":
                    plugin.OnPlacementHook.Dispose();
                    break;
                case "On_DoorCode":
                    plugin.OnDoorCodeHook.Dispose();
                    break;
                case "On_DoorUse":
                    plugin.OnDoorUseHook.Dispose();
                    break;
                case "On_NPCHurt":
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
                case "On_PlayerHurt":
                    plugin.OnPlayerHurtHook.Dispose();
                    break;
                case "On_CombatEntityHurt":
                    plugin.OnCombatEntityHurtHook.Dispose();
                    break;
                case "On_PlayerDied":
                    plugin.OnPlayerDiedHook.Dispose();
                    break;
                case "On_PlayerStartCrafting":
                    plugin.OnPlayerStartCraftingHook.Dispose();
                    break;
                case "On_PlayerTakeRadiation":
                    plugin.OnPlayerTakeRadiationHook.Dispose();
                    break;
                case "On_ServerConsole":
                    plugin.OnServerConsoleHook.Dispose();
                    break;
                case "On_ServerInit":
                    plugin.OnServerInitHook.Dispose();
                    break;
                case "On_ServerShutdown":
                    plugin.OnServerShutdownHook.Dispose();
                    break;
                case "On_Respawn":
                    plugin.OnRespawnHook.Dispose();
                    break;
                case "On_PluginUnload":
                    plugin.Invoke("On_PluginUnload");
                    break;
                default:
                    foundHook = false;
                    break;
                }
                if(foundHook)
                    Logger.LogDebug("Removed hook: " + method);
            }
        }

        #endregion
    }
}

