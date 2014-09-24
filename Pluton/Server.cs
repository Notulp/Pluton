namespace Pluton {
	using System;
	using System.Linq;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class Server {

		// private ItemsBlocks _items;
		// public Pluton.Data data = new Pluton.Data();
		public Dictionary<ulong, Player> Players;
		public Dictionary<ulong, OfflinePlayer> OfflinePlayers;
		public Dictionary<string, LoadOut> LoadOuts;
		public DataStore serverData;
		private static Pluton.Server server;
		public static string server_message_name = "Pluton";
		public Util util = new Util();

		public void Broadcast(string arg) {
			ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(server_message_name) + " " + StringExtensions.QuoteSafe(arg));
		}

		public void BroadcastFrom(string name, string arg) {
			ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(name) + " " + StringExtensions.QuoteSafe(arg));
		}

		public void BroadcastNotice(string s) {
			foreach (Player player in this.Players.Values) {
				//player.Notice(s);
			}
		}

		public Player FindPlayer(string s) {
			BasePlayer player = BasePlayer.Find(s);
			if (player != null)
				return new Player(player);
			return null;
		}

		public static Pluton.Server GetServer() {
			if (server == null) {
				server = new Pluton.Server();
				server.LoadOuts = new Dictionary<string, LoadOut>();
				server.Players = new Dictionary<ulong, Player>();
				server.OfflinePlayers = new Dictionary<ulong, OfflinePlayer>();
				server.serverData = new DataStore("ServerData.ds");
				server.serverData.Load();
				server.LoadOfflinePlayers();
			}
			return server;
		}
		/*
		public System.Collections.Generic.List<string> ChatHistoryMessages {
			get {
				return Fougerite.Data.GetData().chat_history;
			}
		}

		public System.Collections.Generic.List<string> ChatHistoryUsers {
			get {
				return Pluton.Data.GetData().chat_history_username;
			}
		}

		public ItemsBlocks Items {
			get {
				return this._items;
			}
			set {
				this._items = value;
			}
		}*/

		public void LoadOfflinePlayers() {
			if (serverData.GetTable("OfflinePlayers") == null)
				return;
			foreach (KeyValuePair<string, string> kvp in serverData.GetTable("OfflinePlayers")) {
				server.OfflinePlayers.Add(UInt64.Parse(kvp.Key), new OfflinePlayer(kvp.Value));
			}
		}

		public void OnShutdown() {
			foreach (Player player in Players.Values) {
				if (serverData.ContainsKey("OfflinePlayers", player.SteamID)) {
					var op = new OfflinePlayer(serverData.Get("OfflinePlayers", player.SteamID) as string);
					op.Update(player);
					OfflinePlayers[player.GameID] = op;
				} else {
					var op = new OfflinePlayer(player);
					OfflinePlayers.Add(player.GameID, op);
				}
			}
			foreach (OfflinePlayer op2 in OfflinePlayers.Values) {
				serverData.Add("OfflinePlayers", op2.SteamID, op2.ToString());
			}
			serverData.Save();
		}

		public List<Player> ActivePlayers {
			get {
				return (from player in BasePlayer.activePlayerList
					select new Player(player)).ToList();
			}
		}

		public List<Player> SleepingPlayers {
			get {
				return (from player in BasePlayer.sleepingPlayerList
					select new Player(player)).ToList();
			}
		}
	}
}

