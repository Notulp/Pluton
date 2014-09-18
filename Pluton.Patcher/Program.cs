using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Pluton.Patcher {
	class MainClass {

		private static AssemblyDefinition plutonAssembly;
		private static AssemblyDefinition rustAssembly;
		private static TypeDefinition hooksClass;
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

		private static void PlayerConnectedPatch() {
			try {
				TypeDefinition basePlayer = rustAssembly.MainModule.GetType("BasePlayer");
				MethodDefinition bpInit = basePlayer.GetMethod("PlayerInit");
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

		private static void PlayerDisconnectedPatch() {
			try {
				TypeDefinition basePlayer = rustAssembly.MainModule.GetType("BasePlayer");
				MethodDefinition bpDisconnected = basePlayer.GetMethod("OnDisconnected");
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
			plutonAssembly = AssemblyDefinition.ReadAssembly("Pluton.dll");
			hooksClass = plutonAssembly.MainModule.GetType("Pluton.Hooks");

			bool success = true;

			BootstrapAttachPatch();
			PlayerConnectedPatch();
			PlayerDisconnectedPatch();
			ChatPatch();

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
