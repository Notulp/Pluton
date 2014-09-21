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
		public DataStore serverData;
		private static Pluton.Server server;
		public static string server_message_name = "Pluton";
		public Util util = new Util();

		public void Broadcast(string arg) {
			ConsoleSystem.Broadcast("chat.add " + server_message_name + " " + StringExtensions.QuoteSafe(arg));
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
				server.Players = new Dictionary<ulong, Player>();
				server.OfflinePlayers = new Dictionary<ulong, OfflinePlayer>();
				server.serverData = new DataStore("ServerData.ds");
				server.serverData.Load();
			}
			return server;
		}
		/*
		public void Save() {

		}

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

		public void OnServerInit() {
			foreach (KeyValuePair<string, string> kvp in serverData.GetTable("OfflinePlayers")) {
				server.OfflinePlayers.Add(UInt64.Parse(kvp.Key), new OfflinePlayer(kvp.Value));
			}
		}

		public void OnShutdown() {
			foreach (KeyValuePair<ulong, OfflinePlayer> op in OfflinePlayers) {
				serverData.Add("OfflinePlayers", op.Key.ToString(), op.Value.ToString());
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

