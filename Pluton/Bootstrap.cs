using System;
using System.IO;
using UnityEngine;

namespace Pluton {
	public class Bootstrap : MonoBehaviour {

		public static string Version = "0.9.2";

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
			if (Directory.Exists(Util.GetPublicFolder()))
				Directory.CreateDirectory(Util.GetPublicFolder());

			Config.Init();
			Server.GetServer();
			Logger.Init();
			PluginLoader.GetInstance().Init();

			server.official = false;

			if (!server.hostname.ToLower().Contains("pluton"))
				server.hostname = String.Format("{0} [Pluton v.{1}]", server.hostname, Version);
		}
	}
}

