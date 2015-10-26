using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Pluton.Patcher.Reflection;
using System.Linq;

namespace Pluton.Patcher
{
    class MainClass
    {
        public static AssemblyPatcher plutonAssembly;
        public static AssemblyPatcher rustAssembly;
        public static TypePatcher bNPC;
        public static TypePatcher bPlayer;
        public static TypePatcher codeLock;
        public static TypePatcher hooksClass;
        public static TypePatcher itemCrafter;
        public static TypePatcher pLoot;
        public static string Version { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        static bool gendiffs = false;
        static bool newAssCS = false;

        #region patches

        private static void BootstrapAttachPatch()
        {
            var initConfig = rustAssembly.GetType("Bootstrap").GetMethod("Init_Config");
            var attachBootstrap = plutonAssembly.GetType("Pluton.Bootstrap").GetMethod("AttachBootstrap");

            initConfig.InsertCallBefore(0, attachBootstrap);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + initConfig.FriendlyName + ".html", initConfig.PrintAndLink(attachBootstrap));
        }

        private static void ItemsLoadedPatch()
        {
            var initialize = rustAssembly.GetType("ItemManager").GetMethod("Initialize");
            var itemsLoaded = hooksClass.GetMethod("ItemsLoaded");

            initialize.InsertCallBeforeRet(itemsLoaded);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + initialize.FriendlyName + ".html", initialize.PrintAndLink(itemsLoaded));
        }

        private static void BeingHammeredPatch()
        {
            var doAttackShared = rustAssembly.GetType("Hammer").GetMethod("DoAttackShared");
            var getOwnerP = rustAssembly.GetType("HeldEntity").GetMethod("get_ownerPlayer");

            var beingHammered = hooksClass.GetMethod("BeingHammered");

            doAttackShared.InsertAfter(11, Instruction.Create(OpCodes.Ldarg_1))
                .InsertAfter(12, Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallAfter(13, getOwnerP)
                .InsertCallAfter(14, beingHammered);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + doAttackShared.FriendlyName + ".html", doAttackShared.PrintAndLink(beingHammered));
        }

        private static void ChatPatch()
        {
            var say = rustAssembly.GetType("ConVar.Chat").GetMethod("say");
            var onchat = hooksClass.GetMethod("Chat");

            say.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .AppendCall(onchat)
                .Append(Instruction.Create(OpCodes.Ret));
            
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + say.FriendlyName + ".html", say.PrintAndLink(onchat));
        }

        private static void ClientAuthPatch()
        {
            var approve = rustAssembly.GetType("ConnectionAuth").GetMethod("Approve");
            var onClientAuth = hooksClass.GetMethod("ClientAuth");

            approve.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .AppendCall(onClientAuth)
                .Append(Instruction.Create(OpCodes.Ret));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + approve.FriendlyName + ".html", approve.PrintAndLink(onClientAuth));
        }

        private static void ClientConsoleCommandPatch()
        {
            var onClientCommand = rustAssembly.GetType("ConsoleSystem").GetMethod("OnClientCommand");
            var onClientConsoleHook = hooksClass.GetMethod("ClientConsoleCommand");

            onClientCommand.RemoveRange(14, 19)
                .InsertAfter(10, Instruction.Create(OpCodes.Ldloc_1))
                .InsertAfter(11, Instruction.Create(OpCodes.Ldloc_0))
                .InsertCallAfter(12, onClientConsoleHook);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + onClientCommand.FriendlyName + ".html", onClientCommand.PrintAndLink(onClientConsoleHook));
        }

        private static void CombatEntityHurtPatch()
        {
            var combatEnt = rustAssembly.GetType("BaseCombatEntity");
            var onHurtHook = hooksClass.GetMethod("CombatEntityHurt");

            var hurt = combatEnt.GetMethod(methods => {
                return (from method in methods
                    where method.Name == "Hurt" &&
                          method.Parameters[0].Name == "info"
                    select method).First();
            });

            hurt.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .Append(Instruction.Create(OpCodes.Ldarg_2))
                .AppendCall(onHurtHook)
                .Append(Instruction.Create(OpCodes.Ret));
            
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + hurt.FriendlyName + ".html", hurt.PrintAndLink(onHurtHook));
        }

        private static void BuildingBlockDemolishedPatch()
        {
            var buildingBlock = rustAssembly.GetType("BuildingBlock");
            var doDemolish = buildingBlock.GetMethod("DoDemolish");
            var doImmediateDemolish = buildingBlock.GetMethod("DoImmediateDemolish");

            var bpDemolishedHook = hooksClass.GetMethod("BuildingPartDemolished");

            doDemolish.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(bpDemolishedHook);

            doImmediateDemolish.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(bpDemolishedHook);
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + doDemolish.FriendlyName + ".html", doDemolish.PrintAndLink(bpDemolishedHook));
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + doImmediateDemolish.FriendlyName + ".html", doImmediateDemolish.PrintAndLink(bpDemolishedHook));
        }

        private static void CraftingStartPatch()
        {
            var craftItem = rustAssembly.GetType("ItemCrafter").GetMethod("CraftItem");
            var onPlayerStartCraftingHook = hooksClass.GetMethod("PlayerStartCrafting");

            craftItem.Clear()
                .Append(Instruction.Create(OpCodes.Nop))
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg, craftItem.IlProc.Body.Method.Parameters[0]))
                .Append(Instruction.Create(OpCodes.Ldarg, craftItem.IlProc.Body.Method.Parameters[1]))
                .Append(Instruction.Create(OpCodes.Ldarg, craftItem.IlProc.Body.Method.Parameters[2]))
                .Append(Instruction.Create(OpCodes.Ldarg, craftItem.IlProc.Body.Method.Parameters[3]))
                .Append(Instruction.Create(OpCodes.Ldarg, craftItem.IlProc.Body.Method.Parameters[4]))
                .AppendCall(onPlayerStartCraftingHook)
                .Append(Instruction.Create(OpCodes.Ret));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + craftItem.FriendlyName + ".html", craftItem.PrintAndLink(onPlayerStartCraftingHook));
        }

        private static void DoPlacementPatch()
        {
            var createConstruction = rustAssembly.GetType("Construction").GetMethod("CreateConstruction");
            var onPlacement = hooksClass.GetMethod("DoPlacement");

            createConstruction.Clear()
                .Append(Instruction.Create(OpCodes.Nop))
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .Append(Instruction.Create(OpCodes.Ldarg_2))
                .AppendCall(onPlacement)
                .Append(Instruction.Create(OpCodes.Ret));
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + createConstruction.FriendlyName + ".html", createConstruction.PrintAndLink(onPlacement));
        }

        private static void DoorCodePatch()
        {
            var unlockWithCode = rustAssembly.GetType("CodeLock").GetMethod("UnlockWithCode");
            var onClientAuth = hooksClass.GetMethod("DoorCode");

            unlockWithCode.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .AppendCall(onClientAuth)
                .Append(Instruction.Create(OpCodes.Ret));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + unlockWithCode.FriendlyName + ".html", unlockWithCode.PrintAndLink(onClientAuth));
        }

        private static void DoorUsePatch()
        {
            var door = rustAssembly.GetType("Door");
            var close = door.GetMethod("RPC_CloseDoor");
            var open = door.GetMethod("RPC_OpenDoor");
            var doorUse = hooksClass.GetMethod("DoorUse");

            close.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .Append(Instruction.Create(OpCodes.Ldc_I4_0))
                .AppendCall(doorUse)
                .Append(Instruction.Create(OpCodes.Ret));

            open.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .Append(Instruction.Create(OpCodes.Ldc_I4_1))
                .AppendCall(doorUse)
                .Append(Instruction.Create(OpCodes.Ret));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + close.FriendlyName + ".html", close.PrintAndLink(doorUse));
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + open.FriendlyName + ".html", open.PrintAndLink(doorUse));
        }

        private static void GatherPatch()
        {
            var resAttacked = rustAssembly.GetType("BaseResource").GetMethod("OnAttacked");
            var treeAttacked = rustAssembly.GetType("TreeEntity").GetMethod("OnAttacked");

            var onGatheringResource = hooksClass.GetMethod("GatheringBR");
            var onGatheringWood = hooksClass.GetMethod("GatheringTree");

            resAttacked.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .AppendCall(onGatheringResource)
                .Append(Instruction.Create(OpCodes.Ret));
            
            treeAttacked.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .AppendCall(onGatheringWood)
                .Append(Instruction.Create(OpCodes.Ret));
            
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + resAttacked.FriendlyName + ".html", resAttacked.PrintAndLink(onGatheringResource));
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + treeAttacked.FriendlyName + ".html", treeAttacked.PrintAndLink(onGatheringWood));
        }

        private static void NetworkableKillPatch()
        {
            var networkableKill = rustAssembly.GetType("BaseNetworkable").GetMethod("Kill");
            var onNetworkableKilled = hooksClass.GetMethod("NetworkableKill");

            networkableKill.InsertAfter(networkableKill.IlProc.Body.Instructions.Count - 13, Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallAfter(networkableKill.IlProc.Body.Instructions.Count - 13, onNetworkableKilled);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + networkableKill.FriendlyName + ".html", networkableKill.PrintAndLink(onNetworkableKilled));
        }

        private static void NPCDiedPatch()
        {
            var npcKilled = rustAssembly.GetType("BaseNPC").GetMethod("OnKilled");
            var onNPCDied = hooksClass.GetMethod("NPCDied");

            npcKilled.InsertBefore(0, Instruction.Create(OpCodes.Ldarg_0))
                .InsertBefore(1, Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBefore(2, onNPCDied);
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + npcKilled.FriendlyName + ".html", npcKilled.PrintAndLink(onNPCDied));
        }

        private static void PlayerConnectedPatch()
        {
            var basePlayerInit = rustAssembly.GetType("BasePlayer").GetMethod("PlayerInit");
            var onPlayerConnected = hooksClass.GetMethod("PlayerConnected");

            basePlayerInit.InsertBefore(basePlayerInit.IlProc.Body.Instructions.Count - 29, Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBefore(basePlayerInit.IlProc.Body.Instructions.Count - 29, onPlayerConnected);
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + basePlayerInit.FriendlyName + ".html", basePlayerInit.PrintAndLink(onPlayerConnected));
        }

        private static void PlayerDiedPatch()
        {
            var basePlayerDie = rustAssembly.GetType("BasePlayer").GetMethod("Die");
            var onPlayerDied = hooksClass.GetMethod("PlayerDied");

            basePlayerDie.RemoveAt(10)
                .InsertCallBefore(10, onPlayerDied);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + basePlayerDie.FriendlyName + ".html", basePlayerDie.PrintAndLink(onPlayerDied));
        }

        private static void PlayerDisconnectedPatch()
        {
            
            var basePlayerDisconnected = rustAssembly.GetType("BasePlayer").GetMethod("OnDisconnected");
            var onPlayerDisconnected = hooksClass.GetMethod("PlayerDisconnected");

            basePlayerDisconnected.InsertBefore(0, Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBefore(1, onPlayerDisconnected);
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + basePlayerDisconnected.FriendlyName + ".html", basePlayerDisconnected.PrintAndLink(onPlayerDisconnected));
        }

        private static void PlayerTakeRadiationPatch()
        {
            
            var updateRadiation = rustAssembly.GetType("BasePlayer").GetMethod("UpdateRadiation");
            var onPlayerTakeRadiation = hooksClass.GetMethod("PlayerTakeRadiation");

            updateRadiation.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .AppendCall(onPlayerTakeRadiation)
                .Append(Instruction.Create(OpCodes.Ret));
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + onPlayerTakeRadiation.FriendlyName + ".html", onPlayerTakeRadiation.PrintAndLink(onPlayerTakeRadiation));
        }

        private static void PlayerStartLootingPatch()
        {
            var plEntity = pLoot.GetMethod("StartLootingEntity");
            var lootEntity = hooksClass.GetMethod("StartLootingEntity");
            var plPlayer = pLoot.GetMethod("StartLootingPlayer");
            var lootPlayer = hooksClass.GetMethod("StartLootingPlayer");
            var plItem = pLoot.GetMethod("StartLootingItem");
            var lootItem = hooksClass.GetMethod("StartLootingItem");

            plEntity.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(lootEntity);

            plPlayer.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(lootPlayer);

            plItem.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(lootItem);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + plEntity.FriendlyName + ".html", plEntity.PrintAndLink(lootEntity));
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + plPlayer.FriendlyName + ".html", plPlayer.PrintAndLink(lootPlayer));
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + plItem.FriendlyName + ".html", plItem.PrintAndLink(lootItem));
        }

        private static void RespawnPatch()
        {
            
            var respawnAt = rustAssembly.GetType("BasePlayer").GetMethod("RespawnAt");
            var respawn = hooksClass.GetMethod("Respawn");

            respawnAt.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .Append(Instruction.Create(OpCodes.Ldarg_2))
                .AppendCall(respawn)
                .Append(Instruction.Create(OpCodes.Ret));
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + respawnAt.FriendlyName + ".html", respawnAt.PrintAndLink(respawn));
        }

        private static void ServerConsoleCommandPatch()
        {
            var consoleSystemRealm = rustAssembly.GetType("ConsoleSystem").GetNestedType("SystemRealm");
            var serverCmd = consoleSystemRealm.GetMethod(methods => {
                return (from method in methods
                    where method.Parameters.Count == 3 && method.Name == "Normal"
                    select method).FirstOrDefault();
            });
            var onServerConsole = hooksClass.GetMethod("ServerConsoleCommand");

            serverCmd.InsertAfter(12, Instruction.Create(OpCodes.Ldloc_1));
            serverCmd.InsertAfter(13, Instruction.Create(OpCodes.Ldloc_2));
            serverCmd.InsertCallAfter(14, onServerConsole);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + serverCmd.FriendlyName + ".html", serverCmd.PrintAndLink(onServerConsole));
        }

        private static void ShootEvent()
        {
            var baseProjCLProject = rustAssembly.GetType("BaseProjectile").GetMethod("CLProject");
            var onShoot = hooksClass.GetMethod("OnShoot");

            baseProjCLProject.InsertCallBefore(0, onShoot)
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + baseProjCLProject.FriendlyName + ".html", baseProjCLProject.PrintAndLink(onShoot));
        }

        private static void ItemUsed()
        {
            var useItem = rustAssembly.GetType("Item").GetMethod("UseItem");
            var onItemUsed = hooksClass.GetMethod("ItemUsed");

            useItem.InsertCallBefore(0, onItemUsed)
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + useItem.FriendlyName + ".html", useItem.PrintAndLink(onItemUsed));
        }

        private static void Mining()
        {
            var processResource = rustAssembly.GetType("MiningQuarry").GetMethod("ProcessResources");
            var processResourceHook = hooksClass.GetMethod("ProcessResources");

            processResource.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(processResourceHook);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + processResource.FriendlyName + ".html", processResource.PrintAndLink(processResourceHook));
        }

        private static void WeaponThrown()
        {
            var doThrow = rustAssembly.GetType("ThrownWeapon").GetMethod("DoThrow");
            var onThrowing = hooksClass.GetMethod("DoThrow");

            doThrow.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(onThrowing);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + doThrow.FriendlyName + ".html", doThrow.PrintAndLink(onThrowing));
        }

        private static void RocketShootEvent()
        {
            var baseLauncherSVLaunch = rustAssembly.GetType("BaseLauncher").GetMethod("SV_Launch");
            var onRocketShoot = hooksClass.GetMethod("OnRocketShoot");

            baseLauncherSVLaunch.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldloc_S, baseLauncherSVLaunch.IlProc.Body.Variables[7]))
                .InsertCallBeforeRet(onRocketShoot);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + baseLauncherSVLaunch.FriendlyName + ".html", baseLauncherSVLaunch.PrintAndLink(onRocketShoot));
        }

        private static void ConsumeFuel()
        {
            var consumeFuel = rustAssembly.GetType("BaseOven").GetMethod("ConsumeFuel");
            var onConsumeFuel = hooksClass.GetMethod("ConsumeFuel");

            consumeFuel.InsertCallBefore(0, onConsumeFuel)
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_2))
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(0, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + consumeFuel.FriendlyName + ".html", consumeFuel.PrintAndLink(onConsumeFuel));
        }

        private static void ItemPickup()
        {
            var pickupCollectable = rustAssembly.GetType("CollectibleEntity").GetMethod("Pickup");
            var method = hooksClass.GetMethod("Pickup");

            int Position = pickupCollectable.IlProc.Body.Instructions.Count - 36;
            pickupCollectable.InsertCallBefore(Position, method)
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldloc_S, pickupCollectable.IlProc.Body.Variables[3]))
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + pickupCollectable.FriendlyName + ".html", pickupCollectable.PrintAndLink(method));
        }

        private static void FieldsUpdate()
        {
            bPlayer.typeDefinition.GetField("buildingPrivlidges").SetPublic(true);
        }

        private static void PlayerSleep()
        {
            var startSleeping = rustAssembly.GetType("BasePlayer").GetMethod("StartSleeping");
            var onPlayerSleep = hooksClass.GetMethod("PlayerSleep");

            startSleeping.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(onPlayerSleep);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + startSleeping.FriendlyName + ".html", startSleeping.PrintAndLink(onPlayerSleep));
        }

        private static void PlayerWakeUp()
        {
            var endSleeping = rustAssembly.GetType("BasePlayer").GetMethod("EndSleeping");
            var onPlayerWakeUp = hooksClass.GetMethod("PlayerWakeUp");

            endSleeping.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(onPlayerWakeUp);
                

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + endSleeping.FriendlyName + ".html", endSleeping.PrintAndLink(onPlayerWakeUp));
        }

        private static void PlayerLoaded()
        {
            var enterGame = rustAssembly.GetType("BasePlayer").GetMethod("EnterGame");
            var onPlayerLoaded = hooksClass.GetMethod("PlayerLoaded");

            enterGame.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(onPlayerLoaded);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + enterGame.FriendlyName + ".html", enterGame.PrintAndLink(onPlayerLoaded));
        }

        private static void PlayerWounded()
        {
            var basePlayerWound = rustAssembly.GetType("BasePlayer").GetMethod("StartWounded");
            var onPlayerWounded = hooksClass.GetMethod("PlayerWounded");

            basePlayerWound.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(onPlayerWounded);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + basePlayerWound.FriendlyName + ".html", basePlayerWound.PrintAndLink(onPlayerWounded));
        }

        private static void PlayerAssisted()
        {
            var woundAssist = rustAssembly.GetType("BasePlayer").GetMethod("WoundAssist");
            var onPlayerAssisted = hooksClass.GetMethod("PlayerAssisted");

            woundAssist.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(onPlayerAssisted);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + woundAssist.FriendlyName + ".html", woundAssist.PrintAndLink(onPlayerAssisted));
        }

        private static void ItemRepaired()
        {
            var repairItem = rustAssembly.GetType("RepairBench").GetMethod("RepairItem");
            var onItemRepaired = hooksClass.GetMethod("ItemRepaired");

            repairItem.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(onItemRepaired);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + repairItem.FriendlyName + ".html", repairItem.PrintAndLink(onItemRepaired));
        }

        private static void PlayerSyringeSelf()
        {
            var syringeSelf = rustAssembly.GetType("SyringeWeapon").GetMethod("InjectedSelf");
            var onPlayerSyringeSelf = hooksClass.GetMethod("PlayerSyringeSelf");

            syringeSelf.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(onPlayerSyringeSelf);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + syringeSelf.FriendlyName + ".html", syringeSelf.PrintAndLink(onPlayerSyringeSelf));
        }

        private static void PlayerSyringeOther()
        {
            var syringeOther = rustAssembly.GetType("SyringeWeapon").GetMethod("InjectedOther");
            var onPlayerSyringeOther = hooksClass.GetMethod("PlayerSyringeOther");

            syringeOther.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(onPlayerSyringeOther);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + syringeOther.FriendlyName + ".html", syringeOther.PrintAndLink(onPlayerSyringeOther));
        }

        private static void PlayerHealthChange()
        {
            var hpChanged = rustAssembly.GetType("BasePlayer").GetMethod("OnHealthChanged");
            var onPlayerhpChanged = hooksClass.GetMethod("PlayerHealthChangeEvent");

            hpChanged.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_2))
                .InsertCallBeforeRet(onPlayerhpChanged);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + hpChanged.FriendlyName + ".html", hpChanged.PrintAndLink(onPlayerhpChanged));
        }

        private static void PlayerClothingChanged()
        {
            var onClothingChanged = rustAssembly.GetType("PlayerInventory").GetMethod("OnClothingChanged");
            var onPlayerClothingChanged = hooksClass.GetMethod("PlayerClothingChanged");

            onClothingChanged.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertCallBeforeRet(onPlayerClothingChanged);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + onClothingChanged.FriendlyName + ".html", onClothingChanged.PrintAndLink(onPlayerClothingChanged));
        }

        private static void InventoryModificationPatch()
        {
            var ItemContainer = rustAssembly.GetType("ItemContainer");
            var Insert = ItemContainer.GetMethod("Insert");
            var Remove = ItemContainer.GetMethod("Remove");
            var method = hooksClass.GetMethod("ItemAdded");
            var method2 = hooksClass.GetMethod("ItemRemoved");

            int Position = Insert.IlProc.Body.Instructions.Count - 2;
            int Position2 = Remove.IlProc.Body.Instructions.Count - 2;

            Insert.InsertCallBefore(Position, method)
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_0));

            Remove.InsertCallBefore(Position2, method2)
                .InsertBefore(Position2, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(Position2, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + Insert.FriendlyName + ".html", Insert.PrintAndLink(method));
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + Remove.FriendlyName + ".html", Remove.PrintAndLink(method2));
        }

        private static void ItemLoseCondition()
        {
            var loseCondition = rustAssembly.GetType("Item").GetMethod("LoseCondition");
            var onItemLoseCondition = hooksClass.GetMethod("ItemLoseCondition");

            int Position = loseCondition.IlProc.Body.Instructions.Count - 10;
            loseCondition.InsertCallBefore(Position, onItemLoseCondition)
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_1))
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + loseCondition.FriendlyName + ".html", loseCondition.PrintAndLink(onItemLoseCondition));
        }

        private static void LandmineArmed()
        {
            var armLandmine = rustAssembly.GetType("Landmine").GetMethod("Arm");
            var onLandmineArmed = hooksClass.GetMethod("LandmineArmed");

            armLandmine.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertCallBeforeRet(onLandmineArmed);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + armLandmine.FriendlyName + ".html", armLandmine.PrintAndLink(onLandmineArmed));
        }

        private static void LandmineExploded()
        {
            var landmineExplode = rustAssembly.GetType("Landmine").GetMethod("Explode");
            var onLandmineExploded = hooksClass.GetMethod("LandmineExploded");

            int Position = landmineExplode.IlProc.Body.Instructions.Count - 6;
            landmineExplode.InsertCallBefore(Position, onLandmineExploded)
                .InsertBefore(Position, Instruction.Create(OpCodes.Ldarg_0));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + landmineExplode.FriendlyName + ".html", landmineExplode.PrintAndLink(onLandmineExploded));
        }

        private static void LandmineTriggered()
        {
            var landmineTrigger = rustAssembly.GetType("Landmine").GetMethod("Trigger");
            var onLandmineTriggered = hooksClass.GetMethod("LandmineTriggered");

            landmineTrigger.Clear()
                .Append(Instruction.Create(OpCodes.Ldarg_0))
                .Append(Instruction.Create(OpCodes.Ldarg_1))
                .AppendCall(onLandmineTriggered)
                .Append(Instruction.Create(OpCodes.Ret));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + landmineTrigger.FriendlyName + ".html", landmineTrigger.PrintAndLink(onLandmineTriggered));
        }

        private static void ServerInitPatch()
        {
            var srvMgrInit = rustAssembly.GetType("ServerMgr").GetMethod("Initialize");
            var onServerInit = hooksClass.GetMethod("ServerInit");

            srvMgrInit.InsertCallBeforeRet(onServerInit);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + srvMgrInit.FriendlyName + ".html", srvMgrInit.PrintAndLink(onServerInit));
        }

        private static void ServerSavedPatch()
        {
            var doAutomatedSave = rustAssembly.GetType("SaveRestore").GetMethod("DoAutomatedSave");
            var onServerSaved = hooksClass.GetMethod("ServerSaved");

            doAutomatedSave.InsertCallBeforeRet(onServerSaved);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + doAutomatedSave.FriendlyName + ".html", doAutomatedSave.PrintAndLink(onServerSaved));
        }

        private static void ServerShutdownPatch()
        {
            var srvMrgDisable = rustAssembly.GetType("ServerMgr").GetMethod("OnDisable");
            var onServerShutdown = hooksClass.GetMethod("ServerShutdown");

            srvMrgDisable.InsertCallBeforeRet(onServerShutdown);

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + srvMrgDisable.FriendlyName + ".html", srvMrgDisable.PrintAndLink(onServerShutdown));
        }

        private static void SetModdedPatch()
        {
            var srvMgrUpdateInfo = rustAssembly.GetType("ServerMgr").GetMethod("UpdateServerInformation");
            var setModded = hooksClass.GetMethod("SetModded");

            srvMgrUpdateInfo.Clear();

            setModded.AppendCall(setModded)
                .Append(Instruction.Create(OpCodes.Ret));

            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + srvMgrUpdateInfo.FriendlyName + ".html", srvMgrUpdateInfo.PrintAndLink(setModded));
        }

        private static void GiveItemsPatch()
        {
            var giveResFromItem = rustAssembly.GetType("ResourceDispenser").GetMethod("GiveResourceFromItem");
            var onGather = hooksClass.GetMethod("Gathering");

            int iCount = giveResFromItem.IlProc.Body.Instructions.Count;
            giveResFromItem.RemoveRange(iCount - 16, iCount - 1);

            giveResFromItem.InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_0))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_1))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldarg_2))
                .InsertBeforeRet(Instruction.Create(OpCodes.Ldloc_S, giveResFromItem.IlProc.Body.Variables[6]))
                .InsertCallBeforeRet(onGather)
                .InsertBeforeRet(Instruction.Create(OpCodes.Ret));
            
            if (gendiffs && newAssCS)
                File.WriteAllText("diffs" + Path.DirectorySeparatorChar + giveResFromItem.FriendlyName + ".html", giveResFromItem.PrintAndLink(onGather));
        }

        #endregion

        private static void PatchASMCSharp()
        {
            try {
                BootstrapAttachPatch();

                ChatPatch();
                ClientAuthPatch();
                BeingHammeredPatch();
                BuildingBlockDemolishedPatch();
                CombatEntityHurtPatch();
                CraftingStartPatch();
                ConsumeFuel();

                DoPlacementPatch();
                DoorCodePatch();
                DoorUsePatch();

                ItemPickup();
                ItemUsed();
                ItemRepaired();
                ItemLoseCondition();

                LandmineArmed();
                LandmineExploded();
                LandmineTriggered();

                FieldsUpdate();

                GiveItemsPatch();

                PlayerConnectedPatch();
                PlayerDisconnectedPatch();
                PlayerStartLootingPatch();
                PlayerTakeRadiationPatch();
                PlayerDiedPatch();
                PlayerSleep();
                PlayerWakeUp();
                PlayerLoaded();
                PlayerWounded();
                PlayerAssisted();
                PlayerSyringeSelf();
                PlayerSyringeOther();
                PlayerClothingChanged();
                PlayerHealthChange();

                InventoryModificationPatch();

                NetworkableKillPatch();

                NPCDiedPatch();

                RespawnPatch();
                RocketShootEvent();

                ServerShutdownPatch();
                ServerSavedPatch();
                ServerInitPatch();
                SetModdedPatch();

                ClientConsoleCommandPatch();
                ServerConsoleCommandPatch();
                ShootEvent();

                Mining();

                WeaponThrown();

                rustAssembly.CreateType("", "PlutonPatched");
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        public static string GetHtmlDiff(string a, string b)
        {
            var dmp = new DiffMatchPatch.diff_match_patch();
            var diffmain = dmp.diff_main(a, b);
            dmp.diff_cleanupSemantic(diffmain);
            return "<div id='hook_diff'><pre>" + dmp.diff_prettyHtml(diffmain) + "</pre></div>";
        }

        public static int Main(string[] args)
        {
            bool interactive = true;
            if (args.Length > 0)
                interactive = false;

            foreach (string arg in args)
                if (arg.Contains("--generatediffs"))
                    gendiffs = true;

            newAssCS = true;

            Console.WriteLine(string.Format("[( Pluton Patcher v{0} )]", Version));
            try {
                plutonAssembly = AssemblyPatcher.FromFile("Pluton.dll");
                rustAssembly = AssemblyPatcher.FromFile("Assembly-CSharp.dll");
            } catch (FileNotFoundException ex) {
                Console.WriteLine("You are missing " + ex.FileName + " did you move the patcher to the managed folder ?");
                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                }
                return (int)ExitCode.DLL_MISSING;
            } catch (Exception ex) {
                Console.WriteLine("An error occured while reading the assemblies :");
                Console.WriteLine(ex.ToString());
                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                }
                return (int)ExitCode.DLL_READ_ERROR;
            }

            bNPC = rustAssembly.GetType("BaseNPC");
            bPlayer = rustAssembly.GetType("BasePlayer");
            codeLock = rustAssembly.GetType("CodeLock");
            hooksClass = plutonAssembly.GetType("Pluton.Hooks");
            itemCrafter = rustAssembly.GetType("ItemCrafter");
            pLoot = rustAssembly.GetType("PlayerLoot");

            //Check if patching is required
            TypePatcher plutonClass = rustAssembly.GetType("PlutonPatched");
            if (plutonClass == null) {
                try {
                    if (gendiffs) {
                        string hash = String.Empty;
                        using (var sha512 = new System.Security.Cryptography.SHA512Managed())
                            hash = BitConverter.ToString(sha512.ComputeHash(File.ReadAllBytes("Assembly-CSharp.dll"))).Replace("-", "").ToLower();

                        Directory.CreateDirectory("diffs");

                        string hashpath = "diffs" + Path.DirectorySeparatorChar + "lastHash";

                        if (File.Exists(hashpath)) newAssCS = hash != File.ReadAllText(hashpath);

                        if (newAssCS) {
                            foreach (var difffile in Directory.GetFiles("diffs")) {
                                if (difffile.Contains(".htm")) {
                                    string filename = Path.GetFileName(difffile);
                                    string dirname = Path.GetDirectoryName(difffile);
                                    Directory.CreateDirectory(Path.Combine(dirname, "old"));
                                    File.Move(difffile, difffile.Replace(Path.Combine(dirname, filename), Path.Combine(dirname, "old", filename)));
                                }
                            }
                        }

                        if (gendiffs && newAssCS)
                            File.WriteAllText(hashpath, hash);
                    }
                    PatchASMCSharp();
                    Console.WriteLine("Patched Assembly-CSharp !");
                } catch (Exception ex) {
                    interactive = true;
                    Console.WriteLine("An error occured while patching Assembly-CSharp :");
                    Console.WriteLine();
                    Console.WriteLine(ex.Message.ToString());

                    //Normal handle for the others
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace.ToString());
                    Console.WriteLine();

                    if (interactive) {
                        Console.WriteLine("Press any key to continue...");
                    }
                    return (int)ExitCode.ACDLL_GENERIC_PATCH_ERR;
                }
            } else {
                Console.WriteLine("Assembly-CSharp.dll is already patched!");
                return (int)ExitCode.ACDLL_ALREADY_PATCHED;
            }

            try {
                rustAssembly.Write("Assembly-CSharp.dll");
            } catch (Exception ex) {
                Console.WriteLine("An error occured while writing the assembly :");
                Console.WriteLine("Error at: " + ex.TargetSite.Name);
                Console.WriteLine("Error msg: " + ex.Message);

                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                }

                return (int)ExitCode.DLL_WRITE_ERROR;
            }

            //Successfully patched the server
            Console.WriteLine("Completed !");

            if (interactive)
                System.Threading.Thread.Sleep(250);

            return (int)ExitCode.SUCCESS;
        }
    }

    public enum ExitCode : int {
        SUCCESS = 0,
        DLL_MISSING = 10,
        DLL_READ_ERROR = 20,
        DLL_WRITE_ERROR = 21,
        ACDLL_ALREADY_PATCHED = 30,
        ACDLL_GENERIC_PATCH_ERR = 40
    }
}
