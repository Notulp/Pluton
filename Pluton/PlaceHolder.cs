using System;
using Facepunch;
using UnityEngine;

namespace Pluton {
	public class PlaceHolder {

		public void InstallHooks() {
			Hooks.OnPlayerConnected += new Hooks.PlayerConnectedDelegate(OnPlayerConnectedHandler);
			Hooks.OnPlayerDisconnected += new Hooks.PlayerDisconnectedDelegate(OnPlayerDisconnectedHandler);
			Hooks.OnPlayerDied += new Hooks.PlayerDiedDelegate(OnPlayerDiedHandler);
			Hooks.OnPlayerHurt += new Hooks.PlayerHurtDelegate(OnPlayerHurtHandler);
			Hooks.OnNPCDied += new Hooks.NPCDiedDelegate(OnNPCDiedHandler);
			Hooks.OnNPCHurt += new Hooks.NPCHurtDelegate(OnNPCHurtHandler);
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

		public void OnNPCDiedHandler(Events.NPCDeathEvent evt) {
			try {
				Debug.Log(evt.Victim.Prefab + " died in Pluton");
				Debug.Log("DamageType: " + evt.DamageType);
				Debug.Log("IName: " + evt.IName);
				Debug.Log("IPrefab: " + evt.IPrefab);
			} catch (Exception ex) {
				Debug.Log("Maybe HitInfo is null in NPCDeathEvent?");
				Debug.LogException(ex);
			}
		}

		public void OnNPCHurtHandler(Events.NPCHurtEvent evt) {
			try {
				Debug.Log(evt.Victim.Prefab + " got hurt in Pluton");
				Debug.Log("DamageType: " + evt.DamageType);
				Debug.Log("IName: " + evt.IName);
				Debug.Log("IPrefab: " + evt.IPrefab);
			} catch (Exception ex) {
				Debug.Log("Maybe HitInfo is null in NPCHurtEvent?");
				Debug.LogException(ex);
			}
		}

		public void OnPlayerConnectedHandler(Player player) {
			Debug.Log(player.Name + " connected to Pluton");
			Debug.Log("Auth status: " + player.AuthStatus);
		}

		public void OnPlayerDiedHandler(Events.PlayerDeathEvent evt) {
			Debug.Log(evt.Victim.Name + " died in Pluton");
			try {
				Debug.Log("DamageType: " + evt.DamageType);
				Debug.Log("IName: " + evt.IName);
				Debug.Log("IPrefab: " + evt.IPrefab);
			} catch (Exception ex) {
				Debug.Log("Maybe HitInfo is null in PlayerDeathEvent?");
				Debug.LogException(ex);
			}
		}

		public void OnPlayerHurtHandler(Events.PlayerHurtEvent evt) {
			try {
				Debug.Log(evt.Victim.Name + " got hurt in Pluton");
				Debug.Log("DamageType: " + evt.DamageType);
				Debug.Log("IName: " + evt.IName);
				Debug.Log("IPrefab: " + evt.IPrefab);
			} catch (Exception ex) {
				Debug.Log("Maybe HitInfo is null in PlayerHurtEvent?");
				Debug.LogException(ex);
			}
		}

		public void OnPlayerDisconnectedHandler(Player player) {
			Debug.Log(player.Name + " disconnected from Pluton");
		}

		public PlaceHolder() {}
	}
}

