using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Pluton.Patcher {
	class MainClass {

		private static AssemblyDefinition plutonAssembly;
		private static AssemblyDefinition rustAssembly;
		//private static AssemblyDefinition facepunchAssembly;
		private static TypeDefinition hooksClass;
		private static TypeDefinition bAnimal;
		private static TypeDefinition bPlayer;
		private static TypeDefinition bEntity;
		private static TypeDefinition bCorpse;
		private static TypeDefinition bBlock;
		private static TypeDefinition pLoot;
		private static string version = "1.0.0.0";

		private static void BootstrapAttachPatch() {
			try {
				// Call our AttachBootstrap from their, Bootstrap.Start()
				TypeDefinition plutonBootstrap = plutonAssembly.MainModule.GetType("Pluton.Bootstrap");
				TypeDefinition serverInit = rustAssembly.MainModule.GetType("Bootstrap");
				MethodDefinition attachBootstrap = plutonBootstrap.GetMethod("AttachBootstrap");
				MethodDefinition start = serverInit.GetMethod("Start");
				start.Body.GetILProcessor().InsertAfter(start.Body.Instructions[0x03], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(attachBootstrap)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void ClientAuthPatch() {
			try {
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

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		/*private static void ConsolePatch() {
			try {
				TypeDefinition conSys = facepunchAssembly.MainModule.GetType("ConsoleSystem");
				MethodDefinition runInternal = conSys.GetMethod("RunCommandInternal");
				MethodDefinition console = hooksClass.GetMethod("Console");

				CloneMethod(runInternal);
				ILProcessor il = runInternal.Body.GetILProcessor();
				//il.InsertBefore(il.Body.Instructions[0x00], Instruction.Create(OpCodes.Nop)); Console.WriteLine("1");
				il.InsertBefore(il.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0)); Console.WriteLine("2");
				il.InsertAfter(il.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1)); Console.WriteLine("3");
				il.InsertAfter(il.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, facepunchAssembly.MainModule.Import(console))); Console.WriteLine("4");
				il.InsertAfter(il.Body.Instructions[0x03], Instruction.Create(OpCodes.Brtrue, Instruction.Create(OpCodes.Nop))); Console.WriteLine("5");
				il.InsertAfter(il.Body.Instructions[0x04], Instruction.Create(OpCodes.Nop)); Console.WriteLine("6");
				il.InsertAfter(il.Body.Instructions[0x05], Instruction.Create(OpCodes.Ldc_I4_1)); Console.WriteLine("7");
				il.InsertAfter(il.Body.Instructions[0x06], Instruction.Create(OpCodes.Stloc_0)); Console.WriteLine("8");
				il.InsertAfter(il.Body.Instructions[0x07], Instruction.Create(OpCodes.Br)); Console.WriteLine("9");

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}*/

		private static void ChatPatch() {
			try {
				TypeDefinition chat = rustAssembly.MainModule.GetType("chat");
				MethodDefinition say = chat.GetMethod("say");
				MethodDefinition onchat = hooksClass.GetMethod("Chat");

				CloneMethod(say);
				// clear out the method, we will recreate it in Pluton
				say.Body.Instructions.Clear();
				say.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				say.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(onchat)));
				say.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void GatherPatch() {
			try {
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

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void NPCDiedPatch() {
			try {
				MethodDefinition npcdie = bAnimal.GetMethod("Die");
				MethodDefinition npcDied = hooksClass.GetMethod("NPCDied");

				CloneMethod(npcdie);
				ILProcessor iLProcessor = npcdie.Body.GetILProcessor();
				iLProcessor.InsertBefore(npcdie.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(npcdie.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(npcdie.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(npcDied)));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void NPCHurtPatch() {
			try {
				MethodDefinition npchurt = bAnimal.GetMethod("OnAttacked");
				MethodDefinition npcHurt = hooksClass.GetMethod("NPCHurt");

				CloneMethod(npchurt);
				// clear out the method, we will recreate it in Pluton
				npchurt.Body.Instructions.Clear();
				npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
				npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(npcHurt)));
				npchurt.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerConnectedPatch() {
			try {
				MethodDefinition bpInit = bPlayer.GetMethod("PlayerInit");
				MethodDefinition playerConnected = hooksClass.GetMethod("PlayerConnected");

				CloneMethod(bpInit);
				ILProcessor iLProcessor = bpInit.Body.GetILProcessor();

				// Op.Codes.Ldarg_0 would be 'this', the actuall BasePlayer object, but Connection is maybe better for us
				// OpCodes.Ldarg_1 = first(only) parameter of BasePlayer.PlayerInit(Connnection c)
				// 32 = end of the method
				iLProcessor.InsertBefore(bpInit.Body.Instructions[32], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(bpInit.Body.Instructions[32], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerConnected)));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerDiedPatch() {
			try {
				MethodDefinition die = bPlayer.GetMethod("Die");
				MethodDefinition playerDied = hooksClass.GetMethod("PlayerDied");

				CloneMethod(die);
				ILProcessor iLProcessor = die.Body.GetILProcessor();
				iLProcessor.InsertBefore(die.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(die.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(die.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerDied)));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerAttackedPatch() {
			try {
				MethodDefinition hurt = bPlayer.GetMethod("OnAttacked");
				MethodDefinition playerHurt = hooksClass.GetMethod("PlayerHurt");

				CloneMethod(hurt);
				// clear out the method, we will recreate it in Pluton
				ILProcessor iLProcessor = hurt.Body.GetILProcessor();
				iLProcessor.InsertBefore(hurt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(hurt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(hurt.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerHurt)));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerTakeDamagePatch() {
			try {
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
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerTakeDamageOLPatch() {
			try {
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
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerTakeRadiationPatch() {
			try {
				MethodDefinition getRadiated = bPlayer.GetMethod("TakeRadiation");
				MethodDefinition playerTakeRAD = hooksClass.GetMethod("PlayerTakeRadiation");

				CloneMethod(getRadiated);
				ILProcessor iLProcessor = getRadiated.Body.GetILProcessor();
				iLProcessor.InsertBefore(getRadiated.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(getRadiated.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(getRadiated.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerTakeRAD)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerDisconnectedPatch() {
			try {
				MethodDefinition bpDisconnected = bPlayer.GetMethod("OnDisconnected");
				MethodDefinition playerDisconnected = hooksClass.GetMethod("PlayerDisconnected");

				CloneMethod(bpDisconnected);
				ILProcessor iLProcessor = bpDisconnected.Body.GetILProcessor();
				iLProcessor.InsertBefore(bpDisconnected.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bpDisconnected.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(playerDisconnected)));

			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void BuildingBlockAttackedPatch() {
			try {
				MethodDefinition bbAttacked = bBlock.GetMethod("OnAttacked");
				MethodDefinition entAttacked = hooksClass.GetMethod("EntityAttacked");

				CloneMethod(bbAttacked);
				ILProcessor iLProcessor = bbAttacked.Body.GetILProcessor();
				iLProcessor.InsertBefore(bbAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bbAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(bbAttacked.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entAttacked)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void BuildingBlockFrameInitPatch() {
			try {
				MethodDefinition bbFrameInit = bBlock.GetMethod("BecomeFrame");
				MethodDefinition entDeployed = hooksClass.GetMethod("EntityFrameDeployed");

				CloneMethod(bbFrameInit);
				ILProcessor iLProcessor = bbFrameInit.Body.GetILProcessor();
				iLProcessor.InsertBefore(bbFrameInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bbFrameInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entDeployed)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void BuildingBlockBuiltPatch() {
			try {
				MethodDefinition bbBuilt = bBlock.GetMethod("BecomeBuilt");
				MethodDefinition entBuilt = hooksClass.GetMethod("EntityBuilt");

				CloneMethod(bbBuilt);
				ILProcessor iLProcessor = bbBuilt.Body.GetILProcessor();
				iLProcessor.InsertBefore(bbBuilt.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bbBuilt.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entBuilt)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void BuildingBlockUpdatePatch() {
			try {
				MethodDefinition bbBuild = bBlock.GetMethod("DoBuild");
				MethodDefinition entBuild = hooksClass.GetMethod("EntityBuildingUpdate");

				CloneMethod(bbBuild);
				ILProcessor iLProcessor = bbBuild.Body.GetILProcessor();
				iLProcessor.InsertBefore(bbBuild.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bbBuild.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(bbBuild.Body.Instructions[0x01], Instruction.Create(OpCodes.Ldarg_2));
				iLProcessor.InsertAfter(bbBuild.Body.Instructions[0x02], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(entBuild)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void CorpseInitPatch() {
			try {
				MethodDefinition bcInit = bCorpse.GetMethod("InitCorpse");
				MethodDefinition corpseInit = hooksClass.GetMethod("CorpseInit");

				CloneMethod(bcInit);
				ILProcessor iLProcessor = bcInit.Body.GetILProcessor();
				iLProcessor.InsertBefore(bcInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bcInit.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(bcInit.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(corpseInit)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void CorpseAttackedPatch() {
			try {
				MethodDefinition bcAttacked = bCorpse.GetMethod("OnAttacked");
				MethodDefinition corpseHit = hooksClass.GetMethod("CorpseHit");

				CloneMethod(bcAttacked);
				ILProcessor iLProcessor = bcAttacked.Body.GetILProcessor();
				iLProcessor.InsertBefore(bcAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_0));
				iLProcessor.InsertAfter(bcAttacked.Body.Instructions[0x00], Instruction.Create(OpCodes.Ldarg_1));
				iLProcessor.InsertAfter(bcAttacked.Body.Instructions[0x01], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(corpseHit)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void PlayerStartLootingPatch() {
			try {
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
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		private static void ServerShutdownPatch() {
			try {
				TypeDefinition serverMGR = rustAssembly.MainModule.GetType("ServerMgr");
				MethodDefinition disable = serverMGR.GetMethod("OnDisable");
				MethodDefinition shutdown = hooksClass.GetMethod("ServerShutdown");

				CloneMethod(disable);
				ILProcessor iLProcessor = disable.Body.GetILProcessor();
				iLProcessor.InsertBefore(disable.Body.Instructions[0x00], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(shutdown)));
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
			}
		}

		// from fougerite.patcher
		private static MethodDefinition CloneMethod(MethodDefinition orig) {
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

		public static void Main(string[] args) {
			rustAssembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll");
		//	facepunchAssembly = AssemblyDefinition.ReadAssembly("Facepunch.dll");
			plutonAssembly = AssemblyDefinition.ReadAssembly("Pluton.dll");
			hooksClass = plutonAssembly.MainModule.GetType("Pluton.Hooks");
			bAnimal = rustAssembly.MainModule.GetType("BaseAnimal");
			bPlayer = rustAssembly.MainModule.GetType("BasePlayer");
			bEntity = rustAssembly.MainModule.GetType("BaseEntity");
			bCorpse = rustAssembly.MainModule.GetType("BaseCorpse");
			bBlock = rustAssembly.MainModule.GetType("BuildingBlock");
			pLoot = rustAssembly.MainModule.GetType("PlayerLoot");

			bool success = true;

			BootstrapAttachPatch();
			ServerShutdownPatch ();

			ClientAuthPatch();

			//ConsolePatch();
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

			try {
				// TODO: fix, crashed the server
				/*TypeReference type = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.GetType("System.String");
				TypeDefinition item = new TypeDefinition("", "Pluton_Patched", TypeAttributes.AnsiClass | TypeAttributes.Public);
				rustAssembly.MainModule.Types.Add(item);
				TypeReference fieldType = rustAssembly.MainModule.Import(type);
				FieldDefinition definition3 = new FieldDefinition("Version", FieldAttributes.CompilerControlled | FieldAttributes.FamANDAssem | FieldAttributes.Family, fieldType);
				definition3.HasConstant = true;
				definition3.Constant = version;
				rustAssembly.MainModule.GetType("Pluton_Patched").Fields.Add(definition3);*/
				rustAssembly.Write("Assembly-CSharp.dll");
			//	facepunchAssembly.Write("Facepunch.dll");
			} catch (Exception ex) {
				Console.WriteLine("Error at: " + ex.TargetSite.Name);
				Console.WriteLine("Error msg: " + ex.Message);
				success = false;
			}

			if (success) {
				Console.WriteLine("Yay!");
				Console.ReadKey();
			} else {
				Console.WriteLine("Darn!");
				Console.ReadKey();
			}
		}
	}
}
