using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Pluton.Patcher
{
    class MainClass
    {

        private static AssemblyDefinition plutonAssembly;
        private static AssemblyDefinition rustAssembly;
        private static AssemblyDefinition facepunchAssembly;
        private static TypeDefinition hooksClass;
        private static TypeDefinition worldClass;
        private static TypeDefinition bAnimal;
        private static TypeDefinition bPlayer;
        private static TypeDefinition bPlayerMetabolism;
        private static TypeDefinition bCorpse;
        private static TypeDefinition bBlock;
        private static TypeDefinition itemCrafter;
        private static TypeDefinition itemModules;
        private static TypeDefinition pLoot;
        private static TypeDefinition item;
        private static TypeDefinition codeLock;
        private static string version = "1.0.0.10";

        #region patches

        private static void BootstrapAttachPatch()
        {
            // Call our AttachBootstrap from their, Bootstrap.Start()
            TypeDefinition plutonBootstrap = plutonAssembly.MainModule.GetType("Pluton.Bootstrap");
            TypeDefinition serverInit = rustAssembly.MainModule.GetType("Bootstrap");
            MethodDefinition attachBootstrap = plutonBootstrap.GetMethod("AttachBootstrap");
            MethodDefinition init = serverInit.GetMethod("Initialization");

            init.Body.GetILProcessor().InsertAfter(init.Body.Instructions[init.Body.Instructions.Count - 2], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(attachBootstrap)));
        }

        private static void BuildingBlockAttackedPatch()
        {
            MethodDefinition bbAttacked = bBlock.GetMethod("OnAttacked_Destroy");
            MethodDefinition entAttacked = hooksClass.GetMethod("EntityAttacked");

            CloneMethod(bbAttacked);
            ILProcessor iLProcessor = bbAttacked.Body.GetILProcessor();
            iLProcessor.InsertBefore(bbAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bbAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bbAttacked.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entAttacked)));
        }

        private static void BuildingBlockBuiltPatch()
        {
            MethodDefinition bbBuilt = bBlock.GetMethod("StopBeingAFrame");
            MethodDefinition entBuilt = hooksClass.GetMethod("EntityBuilt");

            CloneMethod(bbBuilt);
            ILProcessor iLProcessor = bbBuilt.Body.GetILProcessor();
            iLProcessor.InsertBefore(bbBuilt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bbBuilt.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entBuilt)));
        }

        private static void BuildingBlockFrameInitPatch()
        {
            TypeDefinition planner = itemModules.GetNestedType("Planner");
            MethodDefinition onBuild = planner.GetMethod("OnBuild");
            MethodDefinition entDeployed = hooksClass.GetMethod("EntityFrameDeployed");

            CloneMethod(onBuild);
            ILProcessor iLProcessor = onBuild.Body.GetILProcessor();
            int count = onBuild.Body.Instructions.Count - 18; // after "GameObject gO = this.DoPlacement(placement);"
            iLProcessor.InsertAfter(onBuild.Body.Instructions[count++], Instruction.Create(OpCodes.Ldarg_0)); // planner
            iLProcessor.InsertAfter(onBuild.Body.Instructions[count++], Instruction.Create(OpCodes.Ldarg_1)); // item
            iLProcessor.InsertAfter(onBuild.Body.Instructions[count++], Instruction.Create(OpCodes.Ldarg_2)); // player
            iLProcessor.InsertAfter(onBuild.Body.Instructions[count++], Instruction.Create(OpCodes.Ldloc_3)); // gO (placement)
            iLProcessor.InsertAfter(onBuild.Body.Instructions[count++], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entDeployed)));
        }

        private static void BuildingBlockUpdatePatch()
        {
            // FIXME: OnHammered ?

           /*MethodDefinition bbBuild = bBlock.GetMethod("OnAttacked_Build");
            MethodDefinition entBuild = hooksClass.GetMethod("EntityBuildingUpdate");

            CloneMethod(bbBuild);
            ILProcessor iLProcessor = bbBuild.Body.GetILProcessor();
            iLProcessor.InsertAfter(bbBuild.Body.Instructions[4], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bbBuild.Body.Instructions[5], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bbBuild.Body.Instructions[6], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entBuild)));*/
        }

        private static void CargoPlaneBehaviourPatch()
        {
            TypeDefinition cargoPlane = rustAssembly.MainModule.GetType("CargoPlane");
            MethodDefinition startMtd = cargoPlane.GetMethod("Start");

            startMtd.Body.Instructions.Clear();
            startMtd.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void ChatPatch()
        {
            // FIXME: what the...?

            TypeDefinition chat = rustAssembly.MainModule.GetType("chat");
            MethodDefinition say = chat.GetMethod("say");
            MethodDefinition onchat = hooksClass.GetMethod("Chat");

            CloneMethod(say);
            // clear out the method, we will recreate it in Pluton
            //say.Body.Instructions.Clear();
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
            // clear out the method, we will recreate it in Pluton
            approve.Body.Instructions.Clear();
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(cAuth)));
            approve.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void ClientConsoleCommandPatch()
        {
            TypeDefinition consoleSystem = facepunchAssembly.MainModule.GetType("ConsoleSystem");
            MethodDefinition onClientCmd = consoleSystem.GetMethod("OnClientCommand");
            MethodDefinition onClientConsole = hooksClass.GetMethod("ClientConsoleCommand");

            ILProcessor iLProcessor = onClientCmd.Body.GetILProcessor();

            for (int i = 22; i >= 18; i--)
                iLProcessor.Body.Instructions.RemoveAt(i);

            iLProcessor.InsertAfter(onClientCmd.Body.Instructions[17], Instruction.Create(OpCodes.Ldloc_2));
            iLProcessor.InsertAfter(onClientCmd.Body.Instructions[18], Instruction.Create(OpCodes.Ldloc_1));
            iLProcessor.InsertAfter(onClientCmd.Body.Instructions[19], Instruction.Create(OpCodes.Call, facepunchAssembly.MainModule.Import(onClientConsole)));            
        }

        private static void CorpseAttackedPatch()
        {
            MethodDefinition bcAttacked = bCorpse.GetMethod("OnAttacked");
            MethodDefinition corpseHit = hooksClass.GetMethod("CorpseHit");

            CloneMethod(bcAttacked);
            ILProcessor iLProcessor = bcAttacked.Body.GetILProcessor();
            iLProcessor.InsertBefore(bcAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bcAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bcAttacked.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(corpseHit)));
        }

        private static void CorpseInitPatch()
        {
            MethodDefinition bcInit = bCorpse.GetMethod("InitCorpse");
            MethodDefinition corpseInit = hooksClass.GetMethod("CorpseInit");

            CloneMethod(bcInit);
            ILProcessor iLProcessor = bcInit.Body.GetILProcessor();
            iLProcessor.InsertBefore(bcInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bcInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bcInit.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(corpseInit)));
        }

        private static void CraftingTimePatch()
        {
            MethodDefinition ctInit = itemCrafter.GetMethod("Init");
            MethodDefinition craftTime = hooksClass.GetMethod("CraftingTime");

            CloneMethod(ctInit);
            ILProcessor iLProcessor = ctInit.Body.GetILProcessor();

            iLProcessor.InsertBefore(ctInit.Body.Instructions[9], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(ctInit.Body.Instructions[9], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(craftTime)));
        }

        private static void DoorCodePatch()
        {
            MethodDefinition codeUnlock = codeLock.GetMethod("UnlockWithCode");
            MethodDefinition doorCode = hooksClass.GetMethod("DoorCode");

            CloneMethod(codeUnlock);
            ILProcessor iLProcessor = codeUnlock.Body.GetILProcessor();

            iLProcessor.InsertBefore(codeUnlock.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(codeUnlock.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(codeUnlock.Body.Instructions[1], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(doorCode)));
        }

        private static void GatherPatch()
        {
            TypeDefinition bRes = rustAssembly.MainModule.GetType("BaseResource");
            MethodDefinition gather = bRes.GetMethod("OnAttacked");
            MethodDefinition gathering = hooksClass.GetMethod("Gathering");

            CloneMethod(gather);
            // clear out the method, we will recreate it in Pluton
            gather.Body.Instructions.Clear();
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(gathering)));
            gather.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void NPCDiedPatch()
        {
            MethodDefinition npcdie = bAnimal.GetMethod("Die");
            MethodDefinition npcDied = hooksClass.GetMethod("NPCDied");

            CloneMethod(npcdie);
            ILProcessor iLProcessor = npcdie.Body.GetILProcessor();
            iLProcessor.InsertBefore(npcdie.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(npcdie.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(npcdie.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(npcDied)));
        }

        private static void NPCHurtPatch()
        {
            MethodDefinition npchurt = bAnimal.GetMethod("OnAttacked");
            MethodDefinition npcHurt = hooksClass.GetMethod("NPCHurt");

            CloneMethod(npchurt);
            // clear out the method, we will recreate it in Pluton
            npchurt.Body.Instructions.Clear();
            npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(npcHurt)));
            npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void PlayerAttackedPatch()
        {
            MethodDefinition hurt = bPlayer.GetMethod("OnAttacked");
            MethodDefinition playerHurt = hooksClass.GetMethod("PlayerHurt");

            CloneMethod(hurt);
            // clear out the method, we will recreate it in Pluton
            hurt.Body.Instructions.Clear();
            hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerHurt)));
            hurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void PlayerConnectedPatch()
        {
            MethodDefinition bpInit = bPlayer.GetMethod("PlayerInit");
            MethodDefinition playerConnected = hooksClass.GetMethod("PlayerConnected");

            CloneMethod(bpInit);
            ILProcessor iLProcessor = bpInit.Body.GetILProcessor();

            // Op.Codes.Ldarg_0 would be 'this', the actuall BasePlayer object, but Connection is maybe better for us
            // OpCodes.Ldarg_1 = first(only) parameter of BasePlayer.PlayerInit(Connnection c)
            // 32 = end of the method
            iLProcessor.InsertBefore(bpInit.Body.Instructions[82], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bpInit.Body.Instructions[82], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerConnected)));
        }

        private static void PlayerDiedPatch()
        {
            MethodDefinition die = bPlayer.GetMethod("Die");
            MethodDefinition playerDied = hooksClass.GetMethod("PlayerDied");

            CloneMethod(die);
            ILProcessor iLProcessor = die.Body.GetILProcessor();
            iLProcessor.InsertBefore(die.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(die.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(die.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerDied)));
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

        private static void PlayerTakeDamageOLPatch()
        {
            MethodDefinition hurt = bPlayer.GetMethod("TakeDamageIndicator");
            MethodDefinition playerTakeDMG = hooksClass.GetMethod("PlayerTakeDamageOverload");

            foreach (var method in bPlayer.GetMethods()) {
                if (method.Name == "TakeDamage") {
                    if (method.Parameters.Count == 1) {
                        hurt = method;
                        break;
                    }
                }
            }

            CloneMethod(hurt);
            ILProcessor iLProcessor = hurt.Body.GetILProcessor();
            iLProcessor.InsertBefore(hurt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(hurt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(hurt.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerTakeDMG)));
        }
        private static void RunMetabolismPatch()
        {
            
            MethodDefinition runMetabolism = bPlayerMetabolism.GetMethod("RunMetabolism");
            MethodDefinition metabolismRunHook = hooksClass.GetMethod("RunMetabolism");


            FieldDefinition owner = bPlayerMetabolism.GetField("Owner");
            FieldReference ownerFieldRef = owner as FieldReference;


            CloneMethod(runMetabolism);
            // clear out the method, we will recreate it in Pluton
            runMetabolism.Body.Instructions.Clear();
            runMetabolism.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            runMetabolism.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            runMetabolism.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            runMetabolism.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, ownerFieldRef));
            runMetabolism.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(metabolismRunHook)));
            runMetabolism.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            
        }
        private static void PlayerTakeDamagePatch()
        {
            MethodDefinition hurt = bPlayer.GetMethod("TakeDamageIndicator");
            MethodDefinition playerTakeDMG = hooksClass.GetMethod("PlayerTakeDamage");

            foreach (var method in bPlayer.GetMethods()) {
                if (method.Name == "TakeDamage") {
                    if (method.Parameters.Count == 2) {
                        hurt = method;
                        break;
                    }
                }
            }

            CloneMethod(hurt);
            ILProcessor iLProcessor = hurt.Body.GetILProcessor();
            iLProcessor.InsertBefore(hurt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(hurt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(hurt.Body.Instructions[0x01], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertAfter(hurt.Body.Instructions[0x02], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerTakeDMG)));
        }

        private static void PlayerTakeRadiationPatch()
        {
            MethodDefinition getRadiated = bPlayer.GetMethod("UpdateRadiation");
            MethodDefinition playerTakeRAD = hooksClass.GetMethod("PlayerTakeRadiation");

            CloneMethod(getRadiated);
            ILProcessor iLProcessor = getRadiated.Body.GetILProcessor();
            iLProcessor.InsertBefore(getRadiated.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(getRadiated.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(getRadiated.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerTakeRAD)));
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

        private static void ResourceGatherMultiplierPatch()
        {
            TypeDefinition bRes = rustAssembly.MainModule.GetType("ResourceDispenser");
            MethodDefinition ctInit = bRes.GetMethod("GivePlayerResourceFromItem");
            MethodDefinition gathering = hooksClass.GetMethod("ResourceGatherMultiplier");
            CloneMethod(ctInit);
            ILProcessor iLProcessor = ctInit.Body.GetILProcessor();

            iLProcessor.InsertBefore(ctInit.Body.Instructions[52], Instruction.Create(OpCodes.Ldloc_2));
            iLProcessor.InsertAfter(ctInit.Body.Instructions[52], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(ctInit.Body.Instructions[53], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertAfter(ctInit.Body.Instructions[54], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(gathering)));

            for (int i = 0; i < 8; i++) {
                iLProcessor.Body.Instructions[44 + i] = Instruction.Create(OpCodes.Nop);
            }

        }

        private static void RespawnPatch()
        {
            MethodDefinition respawn = bPlayer.GetMethod("Respawn");
            MethodDefinition spawnEvent = hooksClass.GetMethod("Respawn");

            for (var l = 37; l >= 0; l--) {
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
            TypeDefinition consoleSystem = facepunchAssembly.MainModule.GetType("ConsoleSystem");
            MethodDefinition onServerCmd = consoleSystem.GetMethod("Run");
            //TODO add the function
            MethodDefinition onServerConsole = hooksClass.GetMethod("ServerConsoleCommand");

            ILProcessor iLProcessor = onServerCmd.Body.GetILProcessor();

            for (int i = 9; i >= 6; i--)
                iLProcessor.Body.Instructions.RemoveAt(i);

            iLProcessor.InsertAfter(onServerCmd.Body.Instructions[5], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(onServerCmd.Body.Instructions[6], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(onServerCmd.Body.Instructions[7], Instruction.Create(OpCodes.Call, facepunchAssembly.MainModule.Import(onServerConsole)));            
        }

        private static void ServerInitPatch()
        {
            TypeDefinition servermgr = rustAssembly.MainModule.GetType("ServerMgr");
            MethodDefinition serverInit = servermgr.GetMethod("Initialize");
            MethodDefinition onServerInit = hooksClass.GetMethod("ServerInit");

            CloneMethod(serverInit);
            ILProcessor il = serverInit.Body.GetILProcessor();
            il.InsertBefore(serverInit.Body.Instructions[serverInit.Body.Instructions.Count - 2], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onServerInit)));
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

            CloneMethod(servUpdate);
            ILProcessor il = servUpdate.Body.GetILProcessor();
            il.InsertAfter(servUpdate.Body.Instructions[36], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(setModded)));
        }

        private static void SwapAirdropPatch()
        {
            TypeDefinition eventcmd = rustAssembly.MainModule.GetType("EventCmd");
            TypeDefinition eventPref = rustAssembly.MainModule.GetType("TriggeredEventPrefab");
            MethodDefinition run = eventcmd.GetMethod("run");
            MethodDefinition runEvent = eventPref.GetMethod("RunEvent");

            MethodDefinition getWorld = worldClass.GetMethod("GetWorld");
            MethodDefinition airDrop = worldClass.GetMethod("AirDrop");

            run.Body.Instructions.Clear();
            runEvent.Body.Instructions.Clear();

            run.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(getWorld)));
            run.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(airDrop)));
            run.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            runEvent.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(getWorld)));
            runEvent.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(airDrop)));
            runEvent.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

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
            SwapAirdropPatch();

            BootstrapAttachPatch();
            ServerShutdownPatch();
            ServerInitPatch();
            SetModdedPatch();

            RespawnPatch();

            ClientAuthPatch();

            GatherPatch();
            ChatPatch();

            PlayerDisconnectedPatch();
            PlayerConnectedPatch();

            PlayerStartLootingPatch();

            PlayerTakeRadiationPatch();
            PlayerTakeDamageOLPatch();
            PlayerTakeDamagePatch();
            PlayerAttackedPatch();
            PlayerDiedPatch();

            RunMetabolismPatch();

            NPCDiedPatch();
            NPCHurtPatch();

            CorpseAttackedPatch();
            CorpseInitPatch();

            //BuildingBlockFrameInitPatch();
            BuildingBlockAttackedPatch();
            //BuildingBlockUpdatePatch();
            //BuildingBlockBuiltPatch();

            CraftingTimePatch();
            ResourceGatherMultiplierPatch();

            DoorCodePatch();

            CargoPlaneBehaviourPatch();

            TypeDefinition plutonClass = new TypeDefinition("", "Pluton", TypeAttributes.Public, rustAssembly.MainModule.Import(typeof(Object)));
            rustAssembly.MainModule.Types.Add(plutonClass);
        }

        private static void PatchFacepunch()
        {
            ClientConsoleCommandPatch();
            ServerConsoleCommandPatch();

            TypeDefinition plutonClass = new TypeDefinition("", "Pluton", TypeAttributes.Public, facepunchAssembly.MainModule.Import(typeof(Object)));
            facepunchAssembly.MainModule.Types.Add(plutonClass);
        }

        /*
         * Return values :
         * 10 : File not found
         * 20 : Reading dll error
         * 30 : Server already patched
         * 40 : Generic patch exeption Assembly-CSharp
         * 41 : Generic patch exeption Facepunch
         * 50 : File write error
         */
        public static int Main(string[] args)
        {
            bool interactive = true;
            if (args.Length > 0)
                interactive = false;
            
            Console.WriteLine(string.Format("[( Pluton Patcher v{0} )]", version));
            try {
                rustAssembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll");
                plutonAssembly = AssemblyDefinition.ReadAssembly("Pluton.dll");
                facepunchAssembly = AssemblyDefinition.ReadAssembly("Facepunch.dll");
            } catch (FileNotFoundException ex) {
                Console.WriteLine("You are missing " + ex.FileName + " did you moved the patcher to the managed folder ?");
                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                return 10;
            } catch (Exception ex) {
                Console.WriteLine("An error occured while reading the assemblies :");
                Console.WriteLine(ex.ToString());
                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                return 20;
            }
            
            hooksClass = plutonAssembly.MainModule.GetType("Pluton.Hooks");
            worldClass = plutonAssembly.MainModule.GetType("Pluton.World");
            bAnimal = rustAssembly.MainModule.GetType("BaseAnimal");
            bPlayer = rustAssembly.MainModule.GetType("BasePlayer");
            bPlayerMetabolism = rustAssembly.MainModule.GetType("PlayerMetabolism");
            bCorpse = rustAssembly.MainModule.GetType("BaseCorpse");
            bBlock = rustAssembly.MainModule.GetType("BuildingBlock");
            pLoot = rustAssembly.MainModule.GetType("PlayerLoot");
            itemCrafter = rustAssembly.MainModule.GetType("ItemBlueprint");
            item = rustAssembly.MainModule.GetType("Item");
            codeLock = rustAssembly.MainModule.GetType("CodeLock");
            itemModules = item.GetNestedType("Modules");

            //Check if patching is required
            TypeDefinition plutonClass = rustAssembly.MainModule.GetType("Pluton");
            if (plutonClass == null)
            {
                try
                {
                    PatchASMCSharp();
                    Console.WriteLine("Patched Assembly-CSharp !");
                }
                catch (Exception ex)
                {
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
                    return 40;
                }
            }
            else
                Console.WriteLine("Assembly-CSharp is already patched!");               
            

            plutonClass = facepunchAssembly.MainModule.GetType("Pluton");
            if (plutonClass == null)
            {
                try
                {
                    PatchFacepunch();
                    Console.WriteLine("Patched Facepunch !");
                }
                catch (Exception ex)
                {
                    interactive = true;
                    Console.WriteLine("An error occured while patching Facepunch :");
                    Console.WriteLine();
                    Console.WriteLine(ex.Message.ToString());

                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace.ToString());
                    Console.WriteLine();

                    if (interactive) {
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    return 41;
                }
            }
            else
                Console.WriteLine("Facepunch is already patched!");
                  

            try {
                rustAssembly.Write("Assembly-CSharp.dll");
                facepunchAssembly.Write("Facepunch.dll");
            } catch (Exception ex) {
                Console.WriteLine("An error occured while writing the assembly :");
                Console.WriteLine("Error at: " + ex.TargetSite.Name);
                Console.WriteLine("Error msg: " + ex.Message);

                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }

                return 50;
            }

            //Successfully patched the server
            Console.WriteLine("Completed !");
            System.Threading.Thread.Sleep(250);
            Environment.Exit(0);
            return -1;
        }
    }
}
