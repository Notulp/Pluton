using System;
using Pluton.Events;

namespace Pluton
{
    public interface IPlugin
    {
        string FormatException(Exception ex);

        object Invoke(string method, params object[] args);

        void Load(string code = "");

        void OnAllPluginsLoaded(string s);

        void OnBuildingPartDemolished(BuildingPartDemolishedEvent bpde);

        void OnBuildingPartDestroyed(BuildingPartDestroyedEvent bpde);

        void OnChat(ChatEvent arg);

        void OnClientAuth(AuthEvent ae);

        void OnClientConsole(ClientConsoleEvent ce);

        void OnCombatEntityHurt(CombatEntityHurtEvent he);

        void OnCommand(CommandEvent cmd);

        void OnCommandPermission(CommandPermissionEvent perm);

        void OnConsumeFuel(ConsumeFuelEvent cfe);

        void OnCorpseHurt(CorpseHurtEvent he);

        void OnDoorCode(DoorCodeEvent dc);

        void OnDoorUse(DoorUseEvent due);

        void OnItemAdded(InventoryModEvent ime);

        void OnItemLoseCondition(ItemConditionEvent ice);

        void OnItemPickup(ItemPickupEvent ipe);

        void OnItemRemoved(InventoryModEvent ime);

        void OnItemRepaired(ItemRepairEvent ire);

        void OnItemUsed(ItemUsedEvent uie);

        void OnLootingEntity(EntityLootEvent le);

        void OnLootingItem(ItemLootEvent le);

        void OnLootingPlayer(PlayerLootEvent le);

        void OnMining(MiningQuarry mq);

        void OnNPCHurt(NPCHurtEvent he);

        void OnNPCKilled(NPCDeathEvent de);

        void OnPlacement(BuildingEvent be);

        void OnPlayerAssisted(Player player);

        void OnPlayerClothingChanged(PlayerClothingEvent pce);

        void OnPlayerConnected(Player player);

        void OnPlayerDied(PlayerDeathEvent de);

        void OnPlayerDisconnected(Player player);

        void OnPlayerGathering(GatherEvent ge);

        void OnPlayerHurt(PlayerHurtEvent he);

        void OnPlayerLoaded(Player player);

        void OnPlayerSleep(Player player);

        void OnPlayerStartCrafting(CraftEvent ce);

        void OnPlayerSyringeOther(SyringeUseEvent sue);

        void OnPlayerSyringeSelf(SyringeUseEvent sue);

        void OnPlayerTakeRadiation(PlayerTakeRadsEvent re);

        void OnPlayerWakeUp(Player player);

        void OnPlayerWounded(Player player);

        void OnRespawn(RespawnEvent re);

        void OnRocketShooting(RocketShootEvent rse);

        void OnShooting(ShootEvent se);

        void OnServerConsole(ServerConsoleEvent ce);

        void OnServerInit(string s);

        void OnServerSaved(string s);

        void OnServerShutdown(string s);

        void OnTimerCB(TimedEvent evt);

        void OnWeaponThrow(WeaponThrowEvent wte);
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

