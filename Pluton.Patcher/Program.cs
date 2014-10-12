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
        private static TypeDefinition hooksClass;
        private static TypeDefinition bAnimal;
        private static TypeDefinition bPlayer;
        private static TypeDefinition bCorpse;
        private static TypeDefinition bBlock;
        private static TypeDefinition itemCrafter;
        private static TypeDefinition pLoot;
        private static string version = "1.0.0.3";

        private static void BootstrapAttachPatch()
        {
            // Call our AttachBootstrap from their, Bootstrap.Start()
            TypeDefinition plutonBootstrap = plutonAssembly.MainModule.GetType("Pluton.Bootstrap");
            TypeDefinition serverInit = rustAssembly.MainModule.GetType("Bootstrap");
            MethodDefinition attachBootstrap = plutonBootstrap.GetMethod("AttachBootstrap");
            MethodDefinition start = serverInit.GetMethod("Start");

            // make sure it's not patched yet
            if (start.Body.Instructions[0x05].ToString().Contains("Pluton.Bootstrap::AttachBootstrap")) {
                throw new Exception("Assembly-CSharp is already patched!");
            }

            start.Body.GetILProcessor().InsertAfter(start.Body.Instructions[0x04], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(attachBootstrap)));
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

        private static void ChatPatch()
        {
            TypeDefinition chat = rustAssembly.MainModule.GetType("chat");
            MethodDefinition say = chat.GetMethod("say");
            MethodDefinition onchat = hooksClass.GetMethod("Chat");

            CloneMethod(say);
            // clear out the method, we will recreate it in Pluton
            say.Body.Instructions.Clear();
            say.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            say.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onchat)));
            say.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void GatherPatch()
        {
            TypeDefinition bRes = rustAssembly.MainModule.GetType("BaseResource");
            MethodDefinition gather = bRes.GetMethod("OnAttacked");
            MethodDefinition gathering = hooksClass.GetMethod("Gathering");

            CloneMethod(gather);
            // clear out the method, we will recreate it in Pluton

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

        private static void CraftingTimePatch()
        {
            MethodDefinition ctInit = itemCrafter.GetMethod("Init");
            MethodDefinition craftTime = hooksClass.GetMethod("CraftingTime");

            CloneMethod(ctInit);
            ILProcessor iLProcessor = ctInit.Body.GetILProcessor();

            iLProcessor.InsertBefore(ctInit.Body.Instructions[9], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(ctInit.Body.Instructions[9], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(craftTime)));
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
            iLProcessor.InsertBefore(bpInit.Body.Instructions[32], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bpInit.Body.Instructions[32], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerConnected)));
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

        private static void PlayerDisconnectedPatch()
        {
            MethodDefinition bpDisconnected = bPlayer.GetMethod("OnDisconnected");
            MethodDefinition playerDisconnected = hooksClass.GetMethod("PlayerDisconnected");

            CloneMethod(bpDisconnected);
            ILProcessor iLProcessor = bpDisconnected.Body.GetILProcessor();
            iLProcessor.InsertBefore(bpDisconnected.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bpDisconnected.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerDisconnected)));
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

        private static void BuildingBlockFrameInitPatch()
        {
            MethodDefinition bbFrameInit = bBlock.GetMethod("InitializeAsFrame");
            MethodDefinition entDeployed = hooksClass.GetMethod("EntityFrameDeployed");

            CloneMethod(bbFrameInit);
            ILProcessor iLProcessor = bbFrameInit.Body.GetILProcessor();
            iLProcessor.InsertBefore(bbFrameInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bbFrameInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entDeployed)));
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

        private static void BuildingBlockUpdatePatch()
        {
            MethodDefinition bbBuild = bBlock.GetMethod("OnAttacked_Build");
            MethodDefinition entBuild = hooksClass.GetMethod("EntityBuildingUpdate");

            CloneMethod(bbBuild);
            ILProcessor iLProcessor = bbBuild.Body.GetILProcessor();
            iLProcessor.InsertAfter(bbBuild.Body.Instructions[4], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(bbBuild.Body.Instructions[5], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(bbBuild.Body.Instructions[6], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entBuild)));
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

        private static void PlayerStartLootingPatch()
        {
            FieldDefinition owner = pLoot.GetField("Owner");
            FieldReference ownerFieldRef = owner as FieldReference;
            MethodDefinition plEntity = pLoot.GetMethod("StartLootingEntity");
            MethodDefinition lootEntity = hooksClass.GetMethod("StartLootingEntity");
            MethodDefinition plPlayer = pLoot.GetMethod("StartLootingPlayer");
            MethodDefinition lootPlayer = hooksClass.GetMethod("StartLootingPlayer");
            MethodDefinition plItem = pLoot.GetMethod("StartLootingItem");
            MethodDefinition lootItem = hooksClass.GetMethod("StartLootingItem");

            CloneMethod(plEntity);
            ILProcessor eiLProcessor = plEntity.Body.GetILProcessor();
            eiLProcessor.InsertBefore(plEntity.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            eiLProcessor.InsertAfter(plEntity.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            eiLProcessor.InsertAfter(plEntity.Body.Instructions[0x01], Instruction.Create(OpCodes.Ldfld, ownerFieldRef));
            eiLProcessor.InsertAfter(plEntity.Body.Instructions[0x02], Instruction.Create(OpCodes.Ldarg_1));
            eiLProcessor.InsertAfter(plEntity.Body.Instructions[0x03], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(lootEntity)));

            CloneMethod(plPlayer);
            ILProcessor piLProcessor = plPlayer.Body.GetILProcessor();
            piLProcessor.InsertBefore(plPlayer.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            piLProcessor.InsertAfter(plPlayer.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            piLProcessor.InsertAfter(plPlayer.Body.Instructions[0x01], Instruction.Create(OpCodes.Ldfld, ownerFieldRef));
            piLProcessor.InsertAfter(plPlayer.Body.Instructions[0x02], Instruction.Create(OpCodes.Ldarg_1));
            piLProcessor.InsertAfter(plPlayer.Body.Instructions[0x03], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(lootPlayer)));

            CloneMethod(plItem);
            ILProcessor iiLProcessor = plItem.Body.GetILProcessor();
            iiLProcessor.InsertBefore(plItem.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iiLProcessor.InsertAfter(plItem.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iiLProcessor.InsertAfter(plItem.Body.Instructions[0x01], Instruction.Create(OpCodes.Ldfld, ownerFieldRef));
            iiLProcessor.InsertAfter(plItem.Body.Instructions[0x02], Instruction.Create(OpCodes.Ldarg_1));
            iiLProcessor.InsertAfter(plItem.Body.Instructions[0x03], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(lootItem)));
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

        private static void RespawnPatch()
        {
            MethodDefinition respawn = bPlayer.GetMethod("Respawn");
            MethodDefinition spawnEvent = hooksClass.GetMethod("Respawn");

            for (var l = 39; l >= 0; l--) {
                respawn.Body.Instructions.RemoveAt(l);
            }

            CloneMethod(respawn);
            ILProcessor iLProcessor = respawn.Body.GetILProcessor();
            iLProcessor.InsertBefore(respawn.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertAfter(respawn.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(respawn.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(spawnEvent)));
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

        private static void CargoPlaneBehaviourPatch()
        {
            TypeDefinition cargoPlane = rustAssembly.MainModule.GetType("CargoPlane");
            MethodDefinition startMtd = cargoPlane.GetMethod("Start");

            startMtd.Body.Instructions.Clear();
            startMtd.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

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

        private static void Patch()
        {
            BootstrapAttachPatch();
            ServerShutdownPatch();
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

            NPCDiedPatch();
            NPCHurtPatch();

            CorpseAttackedPatch();
            CorpseInitPatch();

            BuildingBlockFrameInitPatch();
            BuildingBlockAttackedPatch();
            BuildingBlockUpdatePatch();
            BuildingBlockBuiltPatch();

            CraftingTimePatch();
            ResourceGatherMultiplierPatch();

            CargoPlaneBehaviourPatch();
        }

        /*
         * Return values :
         * 10 : File not found
         * 20 : Reading dll error
         * 30 : Server already patched
         * 40 : Generic patch exeption
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
            bAnimal = rustAssembly.MainModule.GetType("BaseAnimal");
            bPlayer = rustAssembly.MainModule.GetType("BasePlayer");
            bCorpse = rustAssembly.MainModule.GetType("BaseCorpse");
            bBlock = rustAssembly.MainModule.GetType("BuildingBlock");
            pLoot = rustAssembly.MainModule.GetType("PlayerLoot");
            itemCrafter = rustAssembly.MainModule.GetType("ItemBlueprint");

            //Try to Patch, return if fails
            try {
                Patch();
            } catch (Exception ex) {
                Console.WriteLine("An error occured while patching Assembly-CSharp :");
                Console.WriteLine();
                Console.WriteLine(ex.Message.ToString());

                //Special case for some errors
                if(ex.Message.ToString() == "Assembly-CSharp is already patched!") {
                    if (interactive) {
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    return 30;
                }

                //Normal handle for the others
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace.ToString());

                if (interactive) {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                return 40;
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

                return 50;
            }

            //Successfully patched the server
            Console.WriteLine("Successfully patched the dll");
            if (interactive) {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            return 0;
        }
    }
}
