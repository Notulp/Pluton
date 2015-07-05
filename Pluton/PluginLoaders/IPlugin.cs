﻿using System;
using Pluton.Events;

namespace Pluton
{
    public interface IPlugin
    {
        string FormatException(Exception ex);

        object Invoke(string method, params object[] args);

        void Load(string code = "");

        void OnAllPluginsLoaded(string s);

        void OnChat(ChatEvent arg);

        void OnClientAuth(AuthEvent ae);

        void OnClientConsole(ClientConsoleEvent ce);

        void OnCombatEntityHurt(CombatEntityHurtEvent he);

        void OnCommand(CommandEvent cmd);

        void OnCommandPermission(CommandPermissionEvent perm);

        void OnCorpseHurt(CorpseHurtEvent he);

        void OnDoorCode(DoorCodeEvent dc);

        void OnDoorUse(DoorUseEvent due);

        void OnLootingEntity(EntityLootEvent le);

        void OnLootingItem(ItemLootEvent le);

        void OnLootingPlayer(PlayerLootEvent le);

        void OnNPCHurt(NPCHurtEvent he);

        void OnNPCKilled(NPCDeathEvent de);

        void OnPlacement(BuildingEvent be);

        void OnPlayerConnected(Player player);

        void OnPlayerDied(PlayerDeathEvent de);

        void OnPlayerDisconnected(Player player);

        void OnPlayerGathering(GatherEvent ge);

        void OnPlayerHurt(PlayerHurtEvent he);

        void OnPlayerStartCrafting(CraftEvent ce);

        void OnPlayerTakeRadiation(PlayerTakeRadsEvent re);

        void OnRespawn(RespawnEvent re);

        void OnServerConsole(ServerConsoleEvent ce);

        void OnServerInit(string s);

        void OnServerShutdown(string s);

        void OnTimerCB(TimedEvent evt);
    }

    public enum PluginState : sbyte
    {
        FailedToLoad = -1,
        NotLoaded = 0,
        Loaded = 1,
        HashNotFound = 2
    }

    public enum PluginType
    {
        Undefined,
        Python,
        JavaScript,
        CSharp,
        CSScript,
        Lua
    }
}

