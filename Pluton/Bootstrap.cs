using System;
using System.IO;
using UnityEngine;

namespace Pluton {
	public class Bootstrap : MonoBehaviour {

		public static string Version = "0.9.0";

		public static void AttachBootstrap() {
			try {
				Console.WriteLine("Pluton Loaded!");
				Init();
			} catch (Exception ex) {
				Debug.LogException(ex);
				Debug.Log("Error while loading Pluton!");
			}
		}

		public static void Init() {
			string FougeriteDirectoryConfig = Path.Combine(Util.GetServerFolder(), "PlutonDirectories.cfg");
			Config.Init(FougeriteDirectoryConfig);
			Server.GetServer();
			Logger.Init();

			server.official = false;
			if (!server.hostname.ToLower().Contains("pluton"))
				server.hostname += " [Pluton mod]";

			var approve = new ProtoBuf.Approval();
			approve.modded = true;

			var ph = new PlaceHolder();
			ph.InstallHooks();
		}
	}
}

