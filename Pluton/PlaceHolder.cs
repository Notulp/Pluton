using System;
using Facepunch;
using UnityEngine;

namespace Pluton {
	public class PlaceHolder {

		public void InstallHooks() {
			Hooks.OnPlayerConnected += new Hooks.PlayerConnectedDelegate(OnPlayerConnectedHandler);
			Hooks.OnPlayerDisconnected += new Hooks.PlayerDisconnectedDelegate(OnPlayerDisconnectedHandler);
			Hooks.OnPlayerDied += new Hooks.PlayerDiedDelegate(OnPlayerDiedHandler);
			Hooks.OnGathering += new Hooks.GatheringDelegate(OnGatheringHandler);
			Hooks.OnChat += new Hooks.ChatDelegate(OnChatHandler);
			Hooks.OnCommand += new Hooks.CommandDelegate(OnCommandHandler);
		}

		public void OnCommandHandler(Player player, string cmd, string[] args) {
			Debug.Log(player.Name + " executed: '/" + cmd + " " + String.Join(" ", args) + "' inside Pluton");
		}

		public void OnChatHandler(ConsoleSystem.Arg arg) {
			Debug.Log(new Player(arg.Player()).Name + " says: " + arg.ArgsStr + " inside Pluton");
		}

		public void OnGatheringHandler(Events.GatherEvent evt) {
			Debug.Log(evt.WeaponName + " " + evt.Prefab + " " + evt.DamageType);
		}

		public void OnPlayerConnectedHandler(Player player) {
			Debug.Log(player.Name + " connected to Pluton");
			Debug.Log("Auth status: " + player.AuthStatus);
		}

		public void OnPlayerDiedHandler(Events.DeathEvent evt) {
			Debug.Log(evt.Victim.Name + " died in Pluton");
			try {
				Debug.Log("DamageType: " + evt.DamageType);
				Debug.Log("IName: " + evt.IName);
				Debug.Log("IPrefab: " + evt.IPrefab);
			} catch (Exception ex) {
				Debug.Log("Maybe HitInfo is null in DeathEvent?");
				Debug.LogException(ex);
			}
		}

		public void OnPlayerDisconnectedHandler(Player player) {
			Debug.Log(player.Name + " disconnected from Pluton");
		}

		public PlaceHolder () { }
	}
}

