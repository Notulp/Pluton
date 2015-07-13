﻿namespace Pluton
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
                throw new FileLoadException("Couldn't initialize " + plugin.Type + " plugin.", 
                    Path.Combine(Path.Combine(pluginDirectory.FullName, plugin.Name), plugin.Name + plugin.Type)
                );
            }

            InstallHooks(plugin);
            Plugins.TryAdd(plugin.Name, plugin);

            // probably make an event here that others can hook?

            Logger.Log(String.Format("[PluginLoader] {0}<{1}> plugin was loaded successfuly.", plugin.Name, plugin.Type));
        }

        public void LoadPlugin(string name, PluginType t)
        {
            PluginLoaders[t].LoadPlugin(name);
        }

        public void LoadPlugins()
        {
            Logger.Log("number of loaders: " + PluginLoaders.Count);
            foreach (IPluginLoader loader in PluginLoaders.Values) {
                loader.LoadPlugins();
            }

            OnAllLoaded.OnNext("");
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
                if (!method.StartsWith("On_", StringComparison.Ordinal) && !method.EndsWith("Callback", StringComparison.Ordinal))
                    continue;

                bool foundHook = true;
                switch (method) {
                case "On_AllPluginsLoaded":
                    plugin.OnAllPluginsLoadedHook = OnAllLoaded.Subscribe(s => plugin.OnAllPluginsLoaded(""));
                    break;
                case "On_Chat":
                    plugin.OnChatHook = Hooks.OnChat.Subscribe(plugin.OnChat);
                    break;
                case "On_ClientAuth":
                    plugin.OnClientAuthHook = Hooks.OnClientAuth.Subscribe(plugin.OnClientAuth);
                    break;
                case "On_ClientConsole":
                    plugin.OnClientConsoleHook = Hooks.OnClientConsole.Subscribe(plugin.OnClientConsole);
                    break;
                case "On_Command":
                    plugin.OnCommandHook = Hooks.OnCommand.Subscribe(plugin.OnCommand);
                    break;
                case "On_CommandPermission":
                    plugin.OnCommandPermissionHook = Hooks.OnCommandPermission.Subscribe(plugin.OnCommandPermission);
                    break;
                case "On_CorpseHurt":
                    plugin.OnCorpseHurtHook = Hooks.OnCorpseHurt.Subscribe(plugin.OnCorpseHurt);
                    break;
                case "On_BuildingComplete":
                    plugin.OnBuildingCompleteHook = Hooks.OnBuildingComplete.Subscribe(plugin.OnBuildingComplete);
                    break;
                case "On_Placement":
                    plugin.OnPlacementHook = Hooks.OnPlacement.Subscribe(plugin.OnPlacement);
                    break;
                case "On_DoorCode":
                    plugin.OnDoorCodeHook = Hooks.OnDoorCode.Subscribe(plugin.OnDoorCode);
                    break;
                case "On_DoorUse":
                    plugin.OnDoorUseHook = Hooks.OnDoorUse.Subscribe(plugin.OnDoorUse);
                    break;
                case "On_NPCHurt":
                    plugin.OnNPCHurtHook = Hooks.OnNPCHurt.Subscribe(plugin.OnNPCHurt);
                    break;
                case "On_NPCKilled":
                    plugin.OnNPCKilledHook = Hooks.OnNPCDied.Subscribe(plugin.OnNPCKilled);
                    break;
                case "On_LootingEntity":
                    plugin.OnLootingEntityHook = Hooks.OnLootingEntity.Subscribe(plugin.OnLootingEntity);
                    break;
                case "On_LootingPlayer":
                    plugin.OnLootingPlayerHook = Hooks.OnLootingPlayer.Subscribe(plugin.OnLootingPlayer);
                    break;
                case "On_LootingItem":
                    plugin.OnLootingItemHook = Hooks.OnLootingItem.Subscribe(plugin.OnLootingItem);
                    break;
                case "On_PlayerConnected":
                    plugin.OnPlayerConnectedHook = Hooks.OnPlayerConnected.Subscribe(plugin.OnPlayerConnected);
                    break;
                case "On_PlayerDisconnected":
                    plugin.OnPlayerDisconnectedHook = Hooks.OnPlayerDisconnected.Subscribe(plugin.OnPlayerDisconnected);
                    break;
                case "On_PlayerGathering":
                    plugin.OnPlayerGatheringHook = Hooks.OnGathering.Subscribe(plugin.OnPlayerGathering);
                    break;
                case "On_PlayerHurt":
                    plugin.OnPlayerHurtHook = Hooks.OnPlayerHurt.Subscribe(plugin.OnPlayerHurt);
                    break;
                case "On_CombatEntityHurt":
                    plugin.OnCombatEntityHurtHook = Hooks.OnCombatEntityHurt.Subscribe(plugin.OnCombatEntityHurt);
                    break;
                case "On_PlayerDied":
                    plugin.OnPlayerDiedHook = Hooks.OnPlayerDied.Subscribe(plugin.OnPlayerDied);
                    break;
                case "On_PlayerStartCrafting":
                    plugin.OnPlayerStartCraftingHook = Hooks.OnPlayerStartCrafting.Subscribe(plugin.OnPlayerStartCrafting);
                    break;
                case "On_PlayerTakeRadiation":
                    plugin.OnPlayerTakeRadiationHook = Hooks.OnPlayerTakeRads.Subscribe(plugin.OnPlayerTakeRadiation);
                    break;
                case "On_PlayerSleep":
                    plugin.OnPlayerSleepHook = Hooks.OnPlayerSleep.Subscribe(plugin.OnPlayerSleep);
                    break;
                case "On_PlayerWakeUp":
                    plugin.OnPlayerWakeUpHook = Hooks.OnPlayerWakeUp.Subscribe(plugin.OnPlayerWakeUp);
                    break;
                case "On_PlayerLoaded":
                    plugin.OnPlayerLoadedHook = Hooks.OnPlayerLoaded.Subscribe(plugin.OnPlayerLoaded);
                    break;
                case "On_PlayerWounded":
                    plugin.OnPlayerWoundedHook = Hooks.OnPlayerWounded.Subscribe(plugin.OnPlayerWounded);
                    break;
                case "On_PlayerAssisted":
                    plugin.OnPlayerAssistedHook = Hooks.OnPlayerAssisted.Subscribe(plugin.OnPlayerAssisted);
                    break;
                case "On_ServerConsole":
                    plugin.OnServerConsoleHook = Hooks.OnServerConsole.Subscribe(plugin.OnServerConsole);
                    break;
                case "On_ServerInit":
                    plugin.OnServerInitHook = Hooks.OnServerInit.Subscribe(plugin.OnServerInit);
                    break;
                case "On_ServerShutdown":
                    plugin.OnServerShutdownHook = Hooks.OnServerShutdown.Subscribe(plugin.OnServerShutdown);
                    break;
                case "On_Respawn":
                    plugin.OnRespawnHook = Hooks.OnRespawn.Subscribe(plugin.OnRespawn);
                    break;
                case "On_Shooting":
                    plugin.OnShootingHook = Hooks.OnShooting.Subscribe(plugin.OnShooting);
                    break;
                case "On_ItemUsed":
                    plugin.OnItemUsedHook = Hooks.OnItemUsed.Subscribe(plugin.OnItemUsed);
                    break;
                case "On_RocketShooting":
                    plugin.OnRocketShootingHook = Hooks.OnRocketShooting.Subscribe(plugin.OnRocketShooting);
                    break;
                case "On_Mining":
                    plugin.OnMiningHook = Hooks.OnMining.Subscribe(plugin.OnMining);
                    break;
                case "On_WeaponThrow":
                    plugin.OnWeaponThrowHook = Hooks.OnWeaponThrow.Subscribe(plugin.OnWeaponThrow);
                    break;
                case "On_ItemPickup":
                    plugin.OnItemPickupHook = Hooks.OnItemPickup.Subscribe(plugin.OnItemPickup);
                    break;
                case "On_ConsumeFuel":
                    plugin.OnConsumeFuelHook = Hooks.OnConsumeFuel.Subscribe(plugin.OnConsumeFuel);
                    break;
                case "On_ItemRepaired":
                    plugin.OnItemRepairedHook = Hooks.OnItemRepaired.Subscribe(plugin.OnItemRepaired);
                    break;
                case "On_PlayerSyringeSelf":
                    plugin.OnPlayerSyringeSelfHook = Hooks.OnPlayerSyringeSelf.Subscribe(plugin.OnPlayerSyringeSelf);
                    break;
                case "On_PlayerSyringeOther":
                    plugin.OnPlayerSyringeOtherHook = Hooks.OnPlayerSyringeOther.Subscribe(plugin.OnPlayerSyringeOther);
                    break;
                case "On_PlayerClothingChanged":
                    plugin.OnPlayerClothingChangedHook = Hooks.OnPlayerClothingChanged.Subscribe(plugin.OnPlayerClothingChanged);
                    break;
                case "On_ItemAdded":
                    plugin.OnItemAddedHook = Hooks.OnItemAdded.Subscribe(plugin.OnItemAdded);
                    break;
                case "On_ItemRemoved":
                    plugin.OnItemRemovedHook = Hooks.OnItemRemoved.Subscribe(plugin.OnItemRemoved);
                    break;
                case "On_PluginInit":
                    plugin.Invoke("On_PluginInit");
                    break;
                case "On_PluginDeinit":
                    break;
                default:
                    foundHook = false;
                    break;
                }
                if(foundHook)
                    Logger.LogDebug("Found hook: " + method);
            }
        }

        public void RemoveHooks(BasePlugin plugin)
        {
            if (plugin.State != PluginState.Loaded)
                return;

            foreach (string method in plugin.Globals) {
                if (!method.StartsWith("On_", StringComparison.Ordinal) && !method.EndsWith("Callback", StringComparison.Ordinal))
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
                case "On_PlayerSleep":
                    plugin.OnPlayerSleepHook.Dispose();
                    break;
                case "On_PlayerWakeUp":
                    plugin.OnPlayerWakeUpHook.Dispose();
                    break;
                case "On_PlayerLoaded":
                    plugin.OnPlayerLoadedHook.Dispose();
                    break;
                case "On_PlayerWounded":
                    plugin.OnPlayerWoundedHook.Dispose();
                    break;
                case "On_PlayerAssisted":
                    plugin.OnPlayerAssistedHook.Dispose();
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
                case "On_Shooting":
                    plugin.OnShootingHook.Dispose();
                    break;
                case "On_ItemUsed":
                    plugin.OnItemUsedHook.Dispose();
                    break;
                case "On_RocketShooting":
                    plugin.OnRocketShootingHook.Dispose();
                    break;
                case "On_Mining":
                    plugin.OnMiningHook.Dispose();
                    break;
                case "On_WeaponThrow":
                    plugin.OnWeaponThrowHook.Dispose();
                    break;
                case "On_ItemPickup":
                    plugin.OnItemPickupHook.Dispose();
                    break;
                case "On_ConsumeFuel":
                    plugin.OnConsumeFuelHook.Dispose();
                    break;
                case "On_ItemRepaired":
                    plugin.OnItemRepairedHook.Dispose();
                    break;
                case "On_PlayerSyringeSelf":
                    plugin.OnPlayerSyringeSelfHook.Dispose();
                    break;
                case "On_PlayerSyringeOther":
                    plugin.OnPlayerSyringeOtherHook.Dispose();
                    break;
                case "On_On_PlayerClothingChanged":
                    plugin.OnPlayerClothingChangedHook.Dispose();
                    break;
                case "On_ItemAdded":
                    plugin.OnItemAddedHook.Dispose();
                    break;
                case "On_ItemRemoved":
                    plugin.OnItemRemovedHook.Dispose();
                    break;
                case "On_PluginInit":
                case "On_PluginDeinit":
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

