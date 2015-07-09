using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Pluton.Patcher
{
    class MainClass
    {
        private static AssemblyDefinition plutonAssembly;
        private static AssemblyDefinition rustAssembly;
        private static TypeDefinition bNPC;
        private static TypeDefinition bPlayer;
        private static TypeDefinition codeLock;
        private static TypeDefinition hooksClass;
        private static TypeDefinition itemCrafter;
        private static TypeDefinition pLoot;
        private static string version = "1.0.0.43";

        #region patches

        private static void BootstrapAttachPatch()
        {
            // Call our AttachBootstrap from their, Bootstrap.Start()
            TypeDefinition plutonBootstrap = plutonAssembly.MainModule.GetType("Pluton.Bootstrap");
            TypeDefinition serverInit = rustAssembly.MainModule.GetType("Bootstrap");
            MethodDefinition attachBootstrap = plutonBootstrap.GetMethod("AttachBootstrap");
            MethodDefinition init = serverInit.GetMethod("Init_Config");

            init.Body.GetILProcessor().InsertBefore(init.Body.Instructions[init.Body.Instructions.Count - 3], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(attachBootstrap)));
        }

        private static void ChatPatch()
        {
            TypeDefinition chat = rustAssembly.MainModule.GetType("ConVar.Chat");
            MethodDefinition say = chat.GetMethod("say");
            MethodDefinition onchat = hooksClass.GetMethod("Chat");

            CloneMethod(say);
            ILProcessor il = say.Body.GetILProcessor();
            il.InsertBefore(say.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(say.Body.Instructions[1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onchat)));
            il.InsertBefore(say.Body.Instructions[2], Instruction.Create(OpCodes.Ret));
        }

        private static void ClientAuthPatch()
        {
            TypeDefinition connAuth = rustAssembly.MainModule.GetType("ConnectionAuth");
            MethodDefinition cAuth = hooksClass.GetMethod("ClientAuth");
            MethodDefinition approve = connAuth.GetMethod("Approve");

            CloneMethod(approve);
            approve.Body.Instructions.Clear();
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(cAuth)));
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void ClientConsoleCommandPatch()
        {
            TypeDefinition consoleSystem = rustAssembly.MainModule.GetType("ConsoleSystem");
            MethodDefinition onClientCmd = consoleSystem.GetMethod("OnClientCommand");
            MethodDefinition onClientConsole = hooksClass.GetMethod("ClientConsoleCommand");

            ILProcessor iLProcessor = onClientCmd.Body.GetILProcessor();

            for (int i = 19; i >= 14; i--)
                iLProcessor.Body.Instructions.RemoveAt(i);

            iLProcessor.InsertAfter(onClientCmd.Body.Instructions[10], Instruction.Create(OpCodes.Ldloc_1));
            iLProcessor.InsertAfter(onClientCmd.Body.Instructions[11], Instruction.Create(OpCodes.Ldloc_0));
            iLProcessor.InsertAfter(onClientCmd.Body.Instructions[12], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onClientConsole)));
        }

        private static void CombatEntityHurtPatch()
        {
            TypeDefinition combatEnt = rustAssembly.MainModule.GetType("BaseCombatEntity");
            MethodDefinition hurtHook = hooksClass.GetMethod("CombatEntityHurt");

            foreach (var hurt in combatEnt.GetMethods()) {
                if (hurt.Name == "Hurt") {
                    if (hurt.Parameters[0].Name == "info") {
                        hurt.Body.Instructions.Clear();

                        hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                        hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(hurtHook)));
                        hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    }
                }
            }
        }

        private static void CraftingStartPatch()
        {
            MethodDefinition craftit = itemCrafter.GetMethod("CraftItem");
            MethodDefinition craftHook = hooksClass.GetMethod("PlayerStartCrafting");

            ILProcessor il = craftit.Body.GetILProcessor();
            il.Body.Instructions.Clear();

            il.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, il.Body.Method.Parameters[3]));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(craftHook)));
            il.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void DoPlacementPatch()
        {
            TypeDefinition construction = rustAssembly.MainModule.GetType("Construction");
            MethodDefinition createConstruction = construction.GetMethod("CreateConstruction");
            MethodDefinition doPlacement = hooksClass.GetMethod("DoPlacement");

            ILProcessor iLProcessor = createConstruction.Body.GetILProcessor();
            iLProcessor.Body.Instructions.Clear();

            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(doPlacement)));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void DoorCodePatch()
        {
            MethodDefinition codeUnlock = codeLock.GetMethod("UnlockWithCode");
            MethodDefinition doorCode = hooksClass.GetMethod("DoorCode");

            codeUnlock.Body.Instructions.Clear();
            codeUnlock.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            codeUnlock.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            codeUnlock.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(doorCode)));
            codeUnlock.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void DoorUsePatch()
        {
            TypeDefinition door = rustAssembly.MainModule.GetType("Door");
            MethodDefinition close = door.GetMethod("RPC_CloseDoor");
            MethodDefinition open = door.GetMethod("RPC_OpenDoor");
            MethodDefinition doorUse = hooksClass.GetMethod("DoorUse");

            ILProcessor iLC = close.Body.GetILProcessor();
            for (int i = close.Body.Instructions.Count - 1; i > 3; i--)
                close.Body.Instructions.RemoveAt(i);

            ILProcessor iLO = open.Body.GetILProcessor();
            for (int i = open.Body.Instructions.Count - 1; i > 3; i--)
                open.Body.Instructions.RemoveAt(i);

            iLC.InsertAfter(close.Body.Instructions[3], Instruction.Create(OpCodes.Nop));
            iLC.InsertAfter(close.Body.Instructions[4], Instruction.Create(OpCodes.Ldarg_0));
            iLC.InsertAfter(close.Body.Instructions[5], Instruction.Create(OpCodes.Ldarg_1));
            iLC.InsertAfter(close.Body.Instructions[6], Instruction.Create(OpCodes.Ldc_I4_0));
            iLC.InsertAfter(close.Body.Instructions[7], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(doorUse)));
            iLC.InsertAfter(close.Body.Instructions[8], Instruction.Create(OpCodes.Ret));

            iLO.InsertAfter(open.Body.Instructions[3], Instruction.Create(OpCodes.Nop));
            iLO.InsertAfter(open.Body.Instructions[4], Instruction.Create(OpCodes.Ldarg_0));
            iLO.InsertAfter(open.Body.Instructions[5], Instruction.Create(OpCodes.Ldarg_1));
            iLO.InsertAfter(open.Body.Instructions[6], Instruction.Create(OpCodes.Ldc_I4_1));
            iLO.InsertAfter(open.Body.Instructions[7], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(doorUse)));
            iLO.InsertAfter(open.Body.Instructions[8], Instruction.Create(OpCodes.Ret));
        }

        private static void GatherPatch()
        {
            TypeDefinition bRes = rustAssembly.MainModule.GetType("BaseResource");
            MethodDefinition gather = bRes.GetMethod("OnAttacked");
            MethodDefinition gatheringBR = hooksClass.GetMethod("GatheringBR");

            TypeDefinition treeEnt = rustAssembly.MainModule.GetType("TreeEntity");
            MethodDefinition gatherWood = treeEnt.GetMethod("OnAttacked");
            MethodDefinition gatheringTree = hooksClass.GetMethod("GatheringTree");

            gather.Body.Instructions.Clear();
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(gatheringBR)));
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            gatherWood.Body.Instructions.Clear();
            gatherWood.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            gatherWood.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            gatherWood.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(gatheringTree)));
            gatherWood.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void NPCDiedPatch()
        {
            MethodDefinition npcdie = bNPC.GetMethod("OnKilled");
            MethodDefinition npcDied = hooksClass.GetMethod("NPCDied");

            CloneMethod(npcdie);
            ILProcessor iLProcessor = npcdie.Body.GetILProcessor();
            iLProcessor.InsertBefore(npcdie.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(npcdie.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(npcdie.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(npcDied)));
        }

        private static void PlayerConnectedPatch()
        {
            MethodDefinition bpInit = bPlayer.GetMethod("PlayerInit");
            MethodDefinition playerConnected = hooksClass.GetMethod("PlayerConnected");

            CloneMethod(bpInit);
            ILProcessor iLProcessor = bpInit.Body.GetILProcessor();
            iLProcessor.InsertBefore(bpInit.Body.Instructions[bpInit.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(bpInit.Body.Instructions[bpInit.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerConnected)));
        }

        private static void PlayerDiedPatch()
        {
            MethodDefinition die = bPlayer.GetMethod("Die");
            MethodDefinition playerDied = hooksClass.GetMethod("PlayerDied");

            CloneMethod(die);
            ILProcessor iLProcessor = die.Body.GetILProcessor();
            Instruction self = Instruction.Create(OpCodes.Ldarg_0);
            iLProcessor.InsertAfter(die.Body.Instructions[0x08], self);
            iLProcessor.InsertAfter(die.Body.Instructions[0x09], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(die.Body.Instructions[0x0a], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerDied)));
            die.Body.Instructions[0x07] = Instruction.Create(OpCodes.Brfalse, self);
        }

        private static void PlayerDisconnectedPatch()
        {
            MethodDefinition bpDisconnected = bPlayer.GetMethod("OnDisconnected");
            MethodDefinition playerDisconnected = hooksClass.GetMethod("PlayerDisconnected");

            CloneMethod(bpDisconnected);
            ILProcessor iLProcessor = bpDisconnected.Body.GetILProcessor();
            iLProcessor.InsertBefore(bpDisconnected.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bpDisconnected.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerDisconnected)));
        }

        private static void PlayerTakeRadiationPatch()
        {
            MethodDefinition getRadiated = bPlayer.GetMethod("UpdateRadiation");
            MethodDefinition playerTakeRAD = hooksClass.GetMethod("PlayerTakeRadiation");

            getRadiated.Body.Instructions.Clear();
            getRadiated.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            getRadiated.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            getRadiated.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerTakeRAD)));
            getRadiated.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void PlayerStartLootingPatch()
        {
            MethodDefinition plEntity = pLoot.GetMethod("StartLootingEntity");
            MethodDefinition lootEntity = hooksClass.GetMethod("StartLootingEntity");
            MethodDefinition plPlayer = pLoot.GetMethod("StartLootingPlayer");
            MethodDefinition lootPlayer = hooksClass.GetMethod("StartLootingPlayer");
            MethodDefinition plItem = pLoot.GetMethod("StartLootingItem");
            MethodDefinition lootItem = hooksClass.GetMethod("StartLootingItem");

            CloneMethod(plEntity);
            ILProcessor eiLProcessor = plEntity.Body.GetILProcessor();
            eiLProcessor.InsertBefore(plEntity.Body.Instructions[plEntity.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_0));
            eiLProcessor.InsertBefore(plEntity.Body.Instructions[plEntity.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(lootEntity)));

            CloneMethod(plPlayer);
            ILProcessor piLProcessor = plPlayer.Body.GetILProcessor();
            piLProcessor.InsertBefore(plPlayer.Body.Instructions[plPlayer.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_0));
            piLProcessor.InsertBefore(plPlayer.Body.Instructions[plPlayer.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(lootPlayer)));

            CloneMethod(plItem);
            ILProcessor iiLProcessor = plItem.Body.GetILProcessor();
            iiLProcessor.InsertBefore(plItem.Body.Instructions[plItem.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_0));
            iiLProcessor.InsertBefore(plItem.Body.Instructions[plItem.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(lootItem)));
        }

        private static void RespawnPatch()
        {
            MethodDefinition respawn = bPlayer.GetMethod("Respawn");
            MethodDefinition spawnEvent = hooksClass.GetMethod("Respawn");

            for (var l = respawn.Body.Instructions.Count - 16; l >= 0; l--) {
                respawn.Body.Instructions.RemoveAt(l);
            }

            CloneMethod(respawn);
            ILProcessor iLProcessor = respawn.Body.GetILProcessor();
            iLProcessor.InsertBefore(respawn.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(respawn.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(respawn.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(spawnEvent)));
        }

        private static void ServerConsoleCommandPatch()
        {
            TypeDefinition consoleSystem = rustAssembly.MainModule.GetType("ConsoleSystem");
            foreach (var i in consoleSystem.GetNestedType("SystemRealm").GetMethods()) {
                if (i.Parameters.Count == 3 && i.Name == "Normal") {
                    MethodDefinition onServerCmd = i;
                    MethodDefinition onServerConsole = hooksClass.GetMethod("ServerConsoleCommand");

                    ILProcessor iLProcessor = onServerCmd.Body.GetILProcessor();
                    iLProcessor.InsertAfter(iLProcessor.Body.Instructions[12], Instruction.Create(OpCodes.Ldloc_1));
                    iLProcessor.InsertAfter(iLProcessor.Body.Instructions[13], Instruction.Create(OpCodes.Ldarg_2));
                    iLProcessor.InsertAfter(iLProcessor.Body.Instructions[14], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onServerConsole)));

                }
            }
        }

        private static void ShootEvent()
        {
            TypeDefinition BaseProjectile = rustAssembly.MainModule.GetType("BaseProjectile");
            MethodDefinition CLProject = BaseProjectile.GetMethod("CLProject");
            MethodDefinition method = hooksClass.GetMethod("OnShoot");
            CloneMethod(CLProject);

            ILProcessor iLProcessor = CLProject.Body.GetILProcessor();
            int Position = CLProject.Body.Instructions.Count - 1;
            iLProcessor.InsertBefore(CLProject.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(CLProject.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(CLProject.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void ItemUsed()
        {
            TypeDefinition Item = rustAssembly.MainModule.GetType("Item");
            MethodDefinition UseItem = Item.GetMethod("UseItem");
            MethodDefinition method = hooksClass.GetMethod("ItemUsed");
            CloneMethod(UseItem);

            ILProcessor iLProcessor = UseItem.Body.GetILProcessor();
            iLProcessor.InsertBefore(UseItem.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(UseItem.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(UseItem.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void Mining()
        {
            TypeDefinition MiningQuarry = rustAssembly.MainModule.GetType("MiningQuarry");
            MethodDefinition ProcessResources = MiningQuarry.GetMethod("ProcessResources");
            MethodDefinition method = hooksClass.GetMethod("ProcessResources");
            CloneMethod(ProcessResources);

            int Position = ProcessResources.Body.Instructions.Count - 6;
            ILProcessor iLProcessor = ProcessResources.Body.GetILProcessor();
            iLProcessor.InsertBefore(ProcessResources.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(ProcessResources.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void WeaponThrown()
        {
            TypeDefinition ThrownWeapon = rustAssembly.MainModule.GetType("ThrownWeapon");
            MethodDefinition DoThrow = ThrownWeapon.GetMethod("DoThrow");
            MethodDefinition method = hooksClass.GetMethod("DoThrow");
            CloneMethod(DoThrow);

            int Position = DoThrow.Body.Instructions.Count - 1;
            ILProcessor iLProcessor = DoThrow.Body.GetILProcessor();
            iLProcessor.InsertBefore(DoThrow.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(DoThrow.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(DoThrow.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void RocketShootEvent()
        {
            TypeDefinition BaseLauncher = rustAssembly.MainModule.GetType("BaseLauncher");
            MethodDefinition SV_Launch = BaseLauncher.GetMethod("SV_Launch");
            MethodDefinition method = hooksClass.GetMethod("OnRocketShoot");
            CloneMethod(SV_Launch);

            ILProcessor iLProcessor = SV_Launch.Body.GetILProcessor();
            int Position = SV_Launch.Body.Instructions.Count - 1;
            iLProcessor.InsertBefore(SV_Launch.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(SV_Launch.Body.Instructions[Position], Instruction.Create(OpCodes.Ldloc_S, SV_Launch.Body.Variables[7]));
            iLProcessor.InsertBefore(SV_Launch.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(SV_Launch.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void ConsumeFuel()
        {
            TypeDefinition ThrownWeapon = rustAssembly.MainModule.GetType("BaseOven");
            MethodDefinition ConsumeFuel = ThrownWeapon.GetMethod("ConsumeFuel");
            MethodDefinition method = hooksClass.GetMethod("ConsumeFuel");
            CloneMethod(ConsumeFuel);

            ILProcessor iLProcessor = ConsumeFuel.Body.GetILProcessor();
            iLProcessor.InsertBefore(ConsumeFuel.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(ConsumeFuel.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(ConsumeFuel.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(ConsumeFuel.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void ItemPickup()
        {
            TypeDefinition CollectibleEntity = rustAssembly.MainModule.GetType("CollectibleEntity");
            MethodDefinition Pickup = CollectibleEntity.GetMethod("Pickup");
            MethodDefinition method = hooksClass.GetMethod("Pickup");
            CloneMethod(Pickup);

            int Position = Pickup.Body.Instructions.Count - 36;
            ILProcessor iLProcessor = Pickup.Body.GetILProcessor();
            iLProcessor.InsertBefore(Pickup.Body.Instructions[Position],
                Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(Pickup.Body.Instructions[Position],
                Instruction.Create(OpCodes.Ldloc_S, Pickup.Body.Variables[3]));
            iLProcessor.InsertBefore(Pickup.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(Pickup.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void FieldsUpdate()
        {
            bPlayer.GetField("buildingPrivlidges").SetPublic(true);
        }

        private static void PlayerSleep()
        {
            MethodDefinition StartSleeping = bPlayer.GetMethod("StartSleeping");
            MethodDefinition method = hooksClass.GetMethod("PlayerSleep");

            int Position = StartSleeping.Body.Instructions.Count - 1;
            ILProcessor iLProcessor = StartSleeping.Body.GetILProcessor();
            iLProcessor.InsertBefore(StartSleeping.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(StartSleeping.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void PlayerWakeUp()
        {
            MethodDefinition EndSleeping = bPlayer.GetMethod("EndSleeping");
            MethodDefinition method = hooksClass.GetMethod("PlayerWakeUp");

            int Position = EndSleeping.Body.Instructions.Count - 1;
            ILProcessor iLProcessor = EndSleeping.Body.GetILProcessor();
            iLProcessor.InsertBefore(EndSleeping.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(EndSleeping.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void PlayerLoaded()
        {
            MethodDefinition EnterGame = bPlayer.GetMethod("EnterGame");
            MethodDefinition method = hooksClass.GetMethod("PlayerLoaded");

            int Position = EnterGame.Body.Instructions.Count - 1;
            ILProcessor iLProcessor = EnterGame.Body.GetILProcessor();
            iLProcessor.InsertBefore(EnterGame.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(EnterGame.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void PlayerWounded()
        {
            MethodDefinition StartWounded = bPlayer.GetMethod("StartWounded");
            MethodDefinition method = hooksClass.GetMethod("PlayerWounded");

            int Position = StartWounded.Body.Instructions.Count - 1;
            ILProcessor ilProcessor = StartWounded.Body.GetILProcessor();
            ilProcessor.InsertBefore(StartWounded.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            ilProcessor.InsertBefore(StartWounded.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void PlayerAssisted()
        {
            MethodDefinition WoundAssist = bPlayer.GetMethod("WoundAssist");
            MethodDefinition method = hooksClass.GetMethod("PlayerAssisted");

            int Position = WoundAssist.Body.Instructions.Count - 1;
            ILProcessor ilProcessor = WoundAssist.Body.GetILProcessor();
            ilProcessor.InsertBefore(WoundAssist.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            ilProcessor.InsertBefore(WoundAssist.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void ItemRepaired()
        {
            TypeDefinition RepairBench = rustAssembly.MainModule.GetType("RepairBench");
            MethodDefinition RepairItem = RepairBench.GetMethod("RepairItem");
            MethodDefinition method = hooksClass.GetMethod("ItemRepaired");

            int Position = RepairItem.Body.Instructions.Count - 1;
            ILProcessor ilProcessor = RepairItem.Body.GetILProcessor();
            ilProcessor.InsertBefore(RepairItem.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            ilProcessor.InsertBefore(RepairItem.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            ilProcessor.InsertBefore(RepairItem.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void PlayerSyringeSelf()
        {
            TypeDefinition SyringeWeapon = rustAssembly.MainModule.GetType("SyringeWeapon");
            MethodDefinition InjectedSelf = SyringeWeapon.GetMethod("InjectedSelf");
            MethodDefinition method = hooksClass.GetMethod("PlayerSyringeSelf");

            int Position = InjectedSelf.Body.Instructions.Count - 1;
            ILProcessor iLProcessor = InjectedSelf.Body.GetILProcessor();
            iLProcessor.InsertBefore(InjectedSelf.Body.Instructions[Position], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(InjectedSelf.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(InjectedSelf.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private static void ServerInitPatch()
        {
            TypeDefinition servermgr = rustAssembly.MainModule.GetType("ServerMgr");
            MethodDefinition serverInit = servermgr.GetMethod("Initialize");
            MethodDefinition onServerInit = hooksClass.GetMethod("ServerInit");

            CloneMethod(serverInit);
            ILProcessor il = serverInit.Body.GetILProcessor();
            il.InsertBefore(serverInit.Body.Instructions[serverInit.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onServerInit)));
        }

        private static void ServerShutdownPatch()
        {
            TypeDefinition serverMGR = rustAssembly.MainModule.GetType("ServerMgr");
            MethodDefinition disable = serverMGR.GetMethod("OnDisable");
            MethodDefinition shutdown = hooksClass.GetMethod("ServerShutdown");

            CloneMethod(disable);
            ILProcessor iLProcessor = disable.Body.GetILProcessor();
            iLProcessor.InsertBefore(disable.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(shutdown)));
        }

        private static void SetModdedPatch()
        {
            TypeDefinition servermgr = rustAssembly.MainModule.GetType("ServerMgr");
            MethodDefinition servUpdate = servermgr.GetMethod("UpdateServerInformation");
            MethodDefinition setModded = hooksClass.GetMethod("SetModded");

            ILProcessor il = servUpdate.Body.GetILProcessor();

            for (var i = 48; i > 7; i--)
                il.Body.Instructions.RemoveAt(i);

            il.InsertAfter(servUpdate.Body.Instructions[7], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(setModded)));
        }

        private static void GiveItemsPatch()
        {
            TypeDefinition disp = rustAssembly.MainModule.GetType("ResourceDispenser");
            //TypeDefinition entComp = rustAssembly.MainModule.GetType("EntityComponent`1");
            MethodDefinition giveFromItem = disp.GetMethod("GiveResourceFromItem");
            //FieldReference fromEnt = entComp.GetField("baseEntity");

            MethodDefinition onGather = hooksClass.GetMethod("Gathering");

            ILProcessor il = giveFromItem.Body.GetILProcessor();
            int count = il.Body.Instructions.Count;
            for (int i = count - 1; i > count - 15; i--) {
                il.Body.Instructions.RemoveAt(i);
            }
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_0));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_1));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldarg_2));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ldloc_S, il.Body.Variables[6]));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onGather)));
            il.InsertAfter(il.Body.Instructions[il.Body.Instructions.Count - 1], Instruction.Create(OpCodes.Ret));
        }

        #endregion

        // from fougerite.patcher
        private static MethodDefinition CloneMethod(MethodDefinition orig)
        {
            MethodDefinition definition = new MethodDefinition(orig.Name + "Original", orig.Attributes, orig.ReturnType);
            foreach (VariableDefinition definition2 in orig.Body.Variables) {
                definition.Body.Variables.Add(definition2);
            }
            foreach (ParameterDefinition definition3 in orig.Parameters) {
                definition.Parameters.Add(definition3);
            }
            foreach (Instruction instruction in orig.Body.Instructions) {
                definition.Body.Instructions.Add(instruction);
            }
            return definition;
        }

        private static void PatchASMCSharp()
        {
            BootstrapAttachPatch();

            ChatPatch();
            ClientAuthPatch();
            CombatEntityHurtPatch();
            CraftingStartPatch();
            ConsumeFuel();

            DoPlacementPatch();
            DoorCodePatch();
            DoorUsePatch();

            ItemPickup();
            ItemUsed();
            ItemRepaired();

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

            NPCDiedPatch();

            RespawnPatch();
            RocketShootEvent();

            ServerShutdownPatch();
            ServerInitPatch();
            SetModdedPatch();

            ClientConsoleCommandPatch();
            ServerConsoleCommandPatch();
            ShootEvent();

            Mining();

            WeaponThrown();

            TypeDefinition plutonClass = new TypeDefinition("", "Pluton", TypeAttributes.Public, rustAssembly.MainModule.Import(typeof(Object)));
            rustAssembly.MainModule.Types.Add(plutonClass);
        }

        public static int Main(string[] args)
        {
            bool interactive = true;
            if (args.Length > 0)
                interactive = false;
            
            Console.WriteLine(string.Format("[( Pluton Patcher v{0} )]", version));
            try {
                plutonAssembly = AssemblyDefinition.ReadAssembly("Pluton.dll");
                rustAssembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll");
            } catch (FileNotFoundException ex) {
                Console.WriteLine("You are missing " + ex.FileName + " did you moved the patcher to the managed folder ?");
                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                return (int)ExitCode.DLL_MISSING;
            } catch (Exception ex) {
                Console.WriteLine("An error occured while reading the assemblies :");
                Console.WriteLine(ex.ToString());
                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                return (int)ExitCode.DLL_READ_ERROR;
            }

            bNPC = rustAssembly.MainModule.GetType("BaseNPC");
            bPlayer = rustAssembly.MainModule.GetType("BasePlayer");
            codeLock = rustAssembly.MainModule.GetType("CodeLock");
            hooksClass = plutonAssembly.MainModule.GetType("Pluton.Hooks");
            itemCrafter = rustAssembly.MainModule.GetType("ItemCrafter");
            pLoot = rustAssembly.MainModule.GetType("PlayerLoot");

            //Check if patching is required
            TypeDefinition plutonClass = rustAssembly.MainModule.GetType("Pluton");
            if (plutonClass == null) {
                try {
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
                        Console.ReadKey();
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
                    Console.ReadKey();
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
