using System;
using Facepunch;
using UnityEngine;

namespace Pluton {
	public class PlaceHolder {

		public void InstallHooks() {
			Hooks.OnPlayerConnected += new Hooks.PlayerConnectedDelegate(OnPlayerConnectedHandler);
			Hooks.OnPlayerDisconnected += new Hooks.PlayerDisconnectedDelegate(OnPlayerDisconnectedHandler);
			Hooks.OnChat += new Hooks.ChatDelegate(OnChatHandler);
			Hooks.OnCommand += new Hooks.CommandDelegate(OnCommandHandler);
		}

		public void OnCommandHandler(Player player, string cmd, string[] args) {
			Debug.Log(player.Name + " executed: '/" + cmd + " " + String.Join(" ", args) + "' inside Pluton");
		}

		public void OnChatHandler(ConsoleSystem.Arg arg) {
			Debug.Log(new Player(arg.Player()).Name + " says: " + arg.ArgsStr + " inside Pluton");
		}

		public void OnPlayerConnectedHandler(Player player) {
			Debug.Log(player.Name + " connected to Pluton");
		}

		public void OnPlayerDisconnectedHandler(Player player) {
			Debug.Log(player.Name + " disconnected from Pluton");
		}

		public PlaceHolder () { }
	}
}

