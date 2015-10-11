namespace Pluton
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    public class Server : Singleton<Server>, ISingleton
    {
        public bool Loaded = false;
        public Dictionary<ulong, Player> Players;
        public Dictionary<ulong, OfflinePlayer> OfflinePlayers;
        public Dictionary<string, LoadOut> LoadOuts;
        public DataStore serverData;
        public static string server_message_name = "Pluton";
        private float craftTimeScale = 1f;

        public void BanPlayer(Player player, string Banner = "Console", string Reason = "You were banned.", Player Sender = null)
        {
            player.Message("You were Banned by: " + Banner);
            if (Sender != null)
            {
                Sender.Message("You banned " + player.Name);
                Sender.Message("Player's IP: " + player.IP);
                Sender.Message("Player's ID: " + player.SteamID);
            }
            player.Kick(Reason);
            foreach (var pl in ActivePlayers.Where(pl => pl.Admin))
            {
                pl.Message(player.Name + " was banned by: " + Banner);
                pl.Message(" Reason: " + Reason);
            }
            BanPlayerIPandID(player.IP, player.SteamID, player.Name, Reason, Banner);
        }

        public void BanPlayerIPandID(string ip, string id, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            var ini = GlobalBanList;
            ini.AddSetting("Ips", ip, name);
            ini.AddSetting("Ids", id, name);
            File.AppendAllText(Path.Combine(Util.GetIdentityFolder(), "GlobalBanList.log"), "[" + DateTime.Now.ToShortDateString() 
                + " " + DateTime.Now.ToShortTimeString() + "] " + name + "|" 
                + ip + "|" +  id + "|" + adminname + "|" + reason + "\r\n");
            ini.Save();
        }

        public void BanPlayerIP(string ip, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            var ini = GlobalBanList;
            ini.AddSetting("Ips", ip, name);
            File.AppendAllText(Path.Combine(Util.GetIdentityFolder(), "GlobalBanList.log"), "[" + DateTime.Now.ToShortDateString()
                + " " + DateTime.Now.ToShortTimeString() + "] " + name + "|"
                + ip + "|" + adminname + "|" + reason + "\r\n");
            ini.Save();
        }

        public void BanPlayerID(string id, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            var ini = GlobalBanList;
            ini.AddSetting("Ids", id, name);
            ini.AddSetting("AdminWhoBanned", name + "|" + id, adminname + "|" + reason);
            ini.Save();
        }

        public void Broadcast(string arg)
        {
            BroadcastFrom(server_message_name, arg);
        }

        public void BroadcastFrom(string name, string arg)
        {
            ConsoleSystem.Broadcast("chat.add", 0, String.Format("{0}: {1}", name.ColorText("fa5"), arg));
        }

        public void BroadcastFrom(ulong playerid, string arg)
        {
            ConsoleSystem.Broadcast("chat.add", playerid, arg, 1);
        }

        public bool CheckDependencies()
        {
            return true;
        }
    
        public void CheckPluginsFolder()
        {
            string path = Util.GetPluginsFolder();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        
        public float CraftingTimeScale
        {
            get
            {
                return craftTimeScale;
            }
            set
            {
                craftTimeScale = value;
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

        public List<string> FindIPsOfName(string name)
        {
            var ini = GlobalBanList;
            var ips = ini.EnumSection("Ips");
            var l = name.ToLower();
            return ips.Where(ip => ini.GetSetting("Ips", ip).ToLower() == l).ToList();
        }

        public List<string> FindIDsOfName(string name)
        {
            var ini = GlobalBanList;
            var ids = ini.EnumSection("Ids");
            var l = name.ToLower();
            return ids.Where(id => ini.GetSetting("Ids", id).ToLower() == l).ToList();
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

        public IniParser GlobalBanList
        {
            get
            {
                var path = Path.Combine(Util.GetIdentityFolder(), "GlobalBanList.ini");
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
                return new IniParser(path);
            }
        }

        public void Initialize()
        {
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
            Instance.CheckPluginsFolder();
        }

        public bool IsBannedID(string id)
        {
            return (GlobalBanList.GetSetting("Ids", id) != null);
        }

        public bool IsBannedIP(string ip)
        {
            return (GlobalBanList.GetSetting("Ips", ip) != null);
        }

        [Obsolete("Server.GetServer() is obsolete, use Server.GetInstance() instead.", false)]
        public static Pluton.Server GetServer()
        {
            return Instance;
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
        
        public int MaxPlayers{
            get{
                return ConVar.Server.maxplayers;
            }
        }

        public bool UnbanByIP(string ip)
        {
            var ini = GlobalBanList;
            if (ini.GetSetting("Ips", ip) == null) return false;
            ini.DeleteSetting("Ips", ip);
            ini.Save();
            return true;
        }

        public bool UnbanByID(string id)
        {
            var ini = GlobalBanList;
            if (ini.GetSetting("Ids", id) == null) return false;
            ini.DeleteSetting("Ids", id);
            ini.Save();
            return true;
        }
        public bool UnbanByName(string name, string UnBanner = "Console", Player Sender = null)
        {
            var ids = FindIDsOfName(name);
            var ips = FindIPsOfName(name);
            if (ids.Count == 0 && ips.Count == 0)
            {
                Sender?.Message("Couldn't find any names matching with " + name);
                return false;
            }
            foreach (var pl in ActivePlayers.Where(pl => pl.Admin))
            {
                pl.Message(name + " was unbanned by: " + UnBanner + " Different matches: " + ids.Count);
            }
            var iptub = ips.Last();
            var idtub = ids.Last();
            var ini = GlobalBanList;
            ini.DeleteSetting("Ips", iptub);
            ini.DeleteSetting("Ids", idtub);
            ini.Save();
            return true;
        }
    }
}

