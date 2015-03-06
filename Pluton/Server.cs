namespace Pluton
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;
    using UnityEngine;

    public class Server : Singleton<Server>, ISingleton
    {
        public bool Loaded = false;
        public Dictionary<ulong, Player> Players;
        public Dictionary<ulong, OfflinePlayer> OfflinePlayers;
        //public Dictionary<string, StructureRecorder.Structure> Structures;
        public Dictionary<string, LoadOut> LoadOuts;
        public DataStore serverData;
        public static string server_message_name = "Pluton";
        private float craftTimeScale = 1f;
        public List<ItemBlueprint> blueprints = new List<ItemBlueprint>();

        public void Broadcast(string arg)
        {
            BroadcastFrom(server_message_name, arg);
        }

        public void BroadcastFrom(string name, string arg)
        {
            ConsoleSystem.Broadcast("chat.add", 0, String.Format("{0}: {1}", name.ColorText("fa5"), arg));
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

        public Player FindPlayer(ulong steamid)
        {
            if (Players.ContainsKey(steamid))
                return Players[steamid];
            return FindPlayer(steamid.ToString());
        }

        public static Player GetPlayer(BasePlayer bp)
        {
            try {
                Player p = GetInstance().FindPlayer(bp.userID);
                if (p != null)
                    return p;
                return new Player(bp);
            } catch (Exception ex) {
                Logger.LogDebug("[Server] GetPlayer: " + ex.Message);
                Logger.LogException(ex);
                return null;
            }
        }

        public void Initialize()
        {
            if (Instance == null) {
                Instance = new Pluton.Server();
                Instance.LoadOuts = new Dictionary<string, LoadOut>();
                //Instance.Structures = new Dictionary<string, StructureRecorder.Structure>();
                Instance.Players = new Dictionary<ulong, Player>();
                Instance.OfflinePlayers = new Dictionary<ulong, OfflinePlayer>();
                Instance.serverData = new DataStore("ServerData.ds");
                Instance.serverData.Load();
                Instance.LoadLoadouts();
                //Instance.LoadStructures();
                //Instance.ReloadBlueprints();
                Instance.LoadOfflinePlayers();
            }
        }

        [Obsolete("Server.GetServer() is obsolete, use Server.GetInstance() instead.", false)]
        public static Pluton.Server GetServer()
        {
            return Instance;
        }

        public float CraftingTimeScale {
            get {
                return craftTimeScale;
            }
            set {
                if(blueprints.Count > 0)
                     Hooks.blueprintsLoaded = true; // used so reloading blueprints won't re-hook in an infinite loop!
                craftTimeScale = value;
                //ReloadBlueprints();
            }
        }

        /*public void ReloadBlueprints() {
            foreach (ItemBlueprint p in blueprints) {
                p.ingredients.Clear();
                p.Init();
                p.time = p.time / CraftingTimeScale;
            }
        }*/

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
                    Instance.OfflinePlayers.Add(UInt64.Parse(entry.Key as string), entry.Value as OfflinePlayer);
                }
            } else {
                Logger.LogWarning("[Server] No OfflinePlayers found!");
            }
            Logger.Log("[Server] " + Instance.OfflinePlayers.Count.ToString() + " offlineplayer loaded!");
        }

        /*public void LoadStructures()
        {
            string path = Util.GetStructuresFolder();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DirectoryInfo structuresPath = new DirectoryInfo(path);
            Structures.Clear();

            foreach (FileInfo file in structuresPath.GetFiles()) {
                if (file.Extension.ToLower() == ".sps") {
                    using (FileStream stream = new FileStream(file.FullName, FileMode.Open)) {
                        BinaryFormatter formatter = new BinaryFormatter();
                        StructureRecorder.Structure structure = (StructureRecorder.Structure)formatter.Deserialize(stream);
                        Structures.Add(file.Name.Substring(0, file.Name.Length - 5), structure);
                    }
                }
            }
            Logger.Log("[Server] " + Structures.Count.ToString() + " structure loaded!");
        }*/

        public void Save()
        {
            OnShutdown();
            foreach (Player p in Players.Values) {
                OfflinePlayers.Remove(p.GameID);
            }
        }

        public bool SendCommand(string command, params object[] args)
        {
            return ConsoleSystem.Run.Server.Normal(command, args);
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

            Util.GetInstance().SaveZones();
            Util.GetInstance().ZoneStore.Save();
        }

        public List<Player> ActivePlayers {
            get {
                return (from player in BasePlayer.activePlayerList
                        select GetPlayer(player)).ToList();
            }
        }

        public List<Player> SleepingPlayers {
            get {
                return (from player in BasePlayer.sleepingPlayerList
                        select GetPlayer(player)).ToList();
            }
        }
    }
}

