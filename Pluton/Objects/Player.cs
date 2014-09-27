using System;
using UnityEngine;

namespace Pluton {
	public class Player {

		public readonly BasePlayer basePlayer;

		public Player(BasePlayer player) {
			basePlayer = player;
			Stats = new PlayerStats(SteamID);
		}

		public static Player Find(string nameOrSteamidOrIP) {
			BasePlayer player = BasePlayer.Find(nameOrSteamidOrIP);
			if (player != null)
				return new Player(player);
			Logger.LogDebug("[Player] Couldn't find player!");
			return null;
		}

		public static Player FindByGameID(ulong steamID) {
			BasePlayer player = BasePlayer.FindByID(steamID);
			if (player != null)
				return new Player(player);
			Logger.LogDebug("[Player] Couldn't find player!");
			return null;
		}

		public static Player FindBySteamID(string steamID) {
			return FindByGameID(UInt64.Parse(steamID));
		}

		public void Ban(string reason = "no reason") {
			ServerUsers.Set(GameID, ServerUsers.UserGroup.Banned, Name, reason);
			ServerUsers.Save();
		}

		public void Kick(string reason = "no reason") {
			Network.Net.sv.Kick(basePlayer.net.connection, reason);
		}

		public void Reject(string reason = "no reason") {
			ConnectionAuth.Reject(basePlayer.net.connection, reason);
		}

		public void Kill() {
			var info = new HitInfo();
			info.damageType = Rust.DamageType.Suicide;
			info.Initiator = basePlayer as BaseEntity;
			basePlayer.Die(info);
		}

		public void MakeNone(string reason = "no reason") {
			ServerUsers.Set(GameID, ServerUsers.UserGroup.None, Name, reason);
			ServerUsers.Save();
		}

		public void MakeModerator(string reason = "no reason") {
			ServerUsers.Set(GameID, ServerUsers.UserGroup.Moderator, Name, reason);
			ServerUsers.Save();
		}

		public void MakeOwner(string reason = "no reason") {
			ServerUsers.Set(GameID, ServerUsers.UserGroup.Owner, Name, reason);
			ServerUsers.Save();
		}

		public void Message(string msg) {
			basePlayer.SendConsoleCommand("chat.add \"" + Server.server_message_name + "\" " + StringExtensions.QuoteSafe(msg));
		}

		public void MessageFrom(string from, string msg) {
			basePlayer.SendConsoleCommand("chat.add \"" + from + "\" " + StringExtensions.QuoteSafe(msg));
		}

		public bool Admin {
			get {
				return basePlayer.IsAdmin();
			}
		}

		public string AuthStatus {
			get {
				return basePlayer.net.connection.authStatus;
			}
		}

		public ulong GameID {
			get {
				return basePlayer.userID;
			}
		}

		public float Health {
			get {
				return basePlayer.Health();
			}
		}

		public Inv Inventory {
			get {
				return new Inv(basePlayer.inventory);
			}
		}

		public string IP {
			get {
				return basePlayer.net.connection.ipaddress;
			}
		}

		public Vector3 Location {
			get {
				return basePlayer.transform.position;
			}
			set {
				basePlayer.transform.position.Set(value.x, value.y, value.z);
			}
		}

		public bool Moderator {
			get {
				return ServerUsers.Is(GameID, ServerUsers.UserGroup.Moderator);
			}
		}

		public string Name {
			get {
				return basePlayer.displayName;
			}
		}

		public bool Owner {
			get {
				return ServerUsers.Is(GameID, ServerUsers.UserGroup.Owner);
			}
		}

		public string OS {
			get {
				return basePlayer.net.connection.os;
			}
		}

		public int Ping {
			get {
				return basePlayer.net.connection.ping;
			}
		}

		public PlayerStats Stats {
			get {
				return Server.GetServer().serverData.Get("PlayerStats", SteamID) as PlayerStats;
			}
			set {
				Server.GetServer().serverData.Add("PlayerStats", SteamID, value);
			}
		}

		public string SteamID {
			get {
				return basePlayer.userID.ToString();
			}
		}

		public float TimeOnline {
			get {
				return basePlayer.net.connection.connectionTime;
			}
		}

		public float X {
			get {
				return basePlayer.transform.position.x;
			}
			set {
				basePlayer.transform.position.Set(value, Y, Z);
			}
		}

		public float Y {
			get {
				return basePlayer.transform.position.y;
			}
			set {
				basePlayer.transform.position.Set(X, value, Z);
			}
		}

		public float Z {
			get {
				return basePlayer.transform.position.z;
			}
			set {
				basePlayer.transform.position.Set(X, Y, value);
			}
		}
	}
}

