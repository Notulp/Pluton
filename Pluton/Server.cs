namespace Pluton
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Server
    {

        public Dictionary<ulong, Player> Players;
        public Dictionary<ulong, OfflinePlayer> OfflinePlayers;
        public Dictionary<string, LoadOut> LoadOuts;
        public DataStore serverData;
        private static Pluton.Server server;
        public static string server_message_name = "Pluton";
        public Util util = new Util();
        private float craftTimeScale = 1f;
        public List<ItemBlueprint> blueprints = new List<ItemBlueprint>();

        public void Broadcast(string arg)
        {
            ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(server_message_name) + " " + StringExtensions.QuoteSafe(arg));
        }

        public void BroadcastFrom(string name, string arg)
        {
            ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(name) + " " + StringExtensions.QuoteSafe(arg));
        }

        public void BroadcastNotice(string s)
        {
            foreach (Player player in this.Players.Values) {
                //player.Notice(s);
            }
        }

        public Player FindPlayer(string s)
        {
            BasePlayer player = BasePlayer.Find(s);
            if (player != null)
                return new Player(player);
            return null;
        }

        public static Pluton.Server GetServer()
        {
            if (server == null) {
                server = new Pluton.Server();
                server.LoadOuts = new Dictionary<string, LoadOut>();
                server.Players = new Dictionary<ulong, Player>();
                server.OfflinePlayers = new Dictionary<ulong, OfflinePlayer>();
                server.serverData = new DataStore("ServerData.ds");
                server.serverData.Load();
                server.LoadLoadouts();
                server.ReloadBlueprints();
                server.LoadOfflinePlayers();
            }
            return server;
        }

        public float CraftingTimeScale {
            get {
                return craftTimeScale;
            }
            set {
                Hooks.blueprintsLoaded = true; // used so reloading blueprints won't re-hook in an infinite loop!
                craftTimeScale = value;
                ReloadBlueprints();
            }
        }

        public void ReloadBlueprints() {
            foreach (ItemBlueprint p in blueprints) {
                p.ingredients.Clear();
                p.Init();
                p.time = p.time / CraftingTimeScale;
            }
        }
        public void LoadLoadouts()
        {
            string path = Util.GetLoadoutFolder();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DirectoryInfo loadoutPath = new DirectoryInfo(path);

            foreach (FileInfo file in loadoutPath.GetFiles()) {
                if (file.Extension == ".ini") {
                    new LoadOut(file.Name.Replace(".ini", ""));
                }
            }
            Logger.Log("[Server] " + LoadOuts.Count.ToString() + " loadout loaded!");
        }

        public void LoadOfflinePlayers()
        {
            Hashtable ht = serverData.GetTable("OfflinePlayers");
            if (ht != null) {
                foreach (DictionaryEntry entry in ht) {
                    server.OfflinePlayers.Add(UInt64.Parse(entry.Key as string), entry.Value as OfflinePlayer);
                }
            } else {
                Logger.LogWarning("[Server] No OfflinePlayers found!");
            }
            Logger.Log("[Server] " + server.OfflinePlayers.Count.ToString() + " offlineplayer loaded!");
        }

        public void Save()
        {
            OnShutdown();
            foreach (Player p in Players.Values) {
                OfflinePlayers.Remove(p.GameID);
            }
        }

        public bool SendCommand(string command, bool wantsReply = true)
        {
            return ConsoleSystem.Run(command, wantsReply);
        }

        public void OnShutdown()
        {
            foreach (Player player in Players.Values) {
                if (serverData.ContainsKey("OfflinePlayers", player.SteamID)) {
                    OfflinePlayer op = serverData.Get("OfflinePlayers", player.SteamID) as OfflinePlayer;
                    op.Update(player);
                    OfflinePlayers[player.GameID] = op;
                } else {
                    OfflinePlayer op = new OfflinePlayer(player);
                    OfflinePlayers.Add(player.GameID, op);
                }
            }
            foreach (OfflinePlayer op2 in OfflinePlayers.Values) {
                serverData.Add("OfflinePlayers", op2.SteamID, op2);
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

