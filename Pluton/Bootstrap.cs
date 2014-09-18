using System;
using UnityEngine;

namespace Pluton {
	public class Bootstrap : MonoBehaviour {

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
			server.official = false;
			server.hostname += " [Pluton mod]";

			var approve = new ProtoBuf.Approval();
			approve.modded = true;

			var ph = new PlaceHolder();
			ph.InstallHooks();
		}
	}
}

