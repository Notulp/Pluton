using System;
using UnityEngine;
using System.Runtime.Serialization;
using Network;

namespace Pluton
{
    [Serializable]
    public class Player : Entity
    {
        [NonSerialized]
        private BasePlayer _basePlayer;

        public readonly ulong GameID;

        public Player(BasePlayer player) : base(player)
        {
            GameID = player.userID;
            _basePlayer = player;
            try {
                Stats = new PlayerStats(SteamID);
            } catch (Exception ex) {
                Logger.LogDebug("[Player] Couldn't load stats!");
                Logger.LogException(ex);
            }
        }

        [OnDeserialized]
        public void OnPlayerDeserialized(StreamingContext context)
        {
            Logger.LogWarning("Deserializing player with id: " + GameID.ToString());
            _basePlayer = BasePlayer.FindByID(GameID);
            if (_basePlayer == null)
                Logger.LogWarning("_basePlayer is <null>, is the player offline?");
            else
                Logger.LogWarning("basePlayer found: " + _basePlayer.displayName);
        }

        public static Player Find(string nameOrSteamidOrIP)
        {
            BasePlayer player = BasePlayer.Find(nameOrSteamidOrIP);
            if (player != null)
                return Server.GetPlayer(player);
            Logger.LogDebug("[Player] Couldn't find player!");
            return null;
        }

        public static Player FindByGameID(ulong steamID)
        {
            BasePlayer player = BasePlayer.FindByID(steamID);
            if (player != null)
                return Server.GetPlayer(player);
            Logger.LogDebug("[Player] Couldn't find player!");
            return null;
        }

        public static Player FindBySteamID(string steamID)
        {
            return FindByGameID(UInt64.Parse(steamID));
        }

        public void Ban(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.Banned, Name, reason);
            ServerUsers.Save();
            Kick("Banned!");
        }

        public void Kick(string reason = "no reason")
        {
            Network.Net.sv.Kick(basePlayer.net.connection, reason);
        }

        public void Reject(string reason = "no reason")
        {
            ConnectionAuth.Reject(basePlayer.net.connection, reason);
        }

        public Vector3 GetLookPoint(float maxDist = 500f)
        {
            return GetLookHit(maxDist).point;
        }

        public RaycastHit GetLookHit(float maxDist = 500f)
        {
            RaycastHit hit;
            Ray orig = basePlayer.eyes.Ray();
            Physics.Raycast(orig, out hit, maxDist, Physics.AllLayers);
            return hit;
        }

        public Player GetLookPlayer(float maxDist = 500f)
        {
            RaycastHit hit = GetLookHit(maxDist);
            if (hit.collider != null) {
                BasePlayer basePlayer = hit.collider.GetComponent<BasePlayer>();
                if (basePlayer != null) {
                    return Server.GetPlayer(basePlayer);
                }
            }
            return null;
        }

        public BuildingPart GetLookBuildingPart(float maxDist = 500f)
        {
            RaycastHit hit = GetLookHit(maxDist);
            if (hit.collider != null) {
                BuildingBlock buildingBlock = hit.collider.GetComponent<BuildingBlock>();
                if (buildingBlock != null) {
                    return new BuildingPart(buildingBlock);
                }
            }
            return null;
        }

        public override void Kill()
        {
            var info = new HitInfo();
            info.damageTypes.Add(Rust.DamageType.Suicide, Single.MaxValue);
            info.Initiator = baseEntity;
            basePlayer.Die(info);
        }

        public void MakeNone(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.None, Name, reason);
            basePlayer.net.connection.authLevel = 0;
            ServerUsers.Save();
        }

        public void MakeModerator(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.Moderator, Name, reason);
            basePlayer.net.connection.authLevel = 1;
            ServerUsers.Save();
        }

        public void MakeOwner(string reason = "no reason")
        {
            ServerUsers.Set(GameID, ServerUsers.UserGroup.Owner, Name, reason);
            basePlayer.net.connection.authLevel = 2;
            ServerUsers.Save();
        }

        public void Message(string msg)
        {
            MessageFrom(Server.server_message_name, msg);
        }

        public void MessageFrom(string from, string msg)
        {
            basePlayer.SendConsoleCommand("chat.add", 0, from.ColorText("fa5") + ": " + msg);
        }

        public void ConsoleMessage(string msg)
        {
            basePlayer.SendConsoleCommand("echo", msg);
        }

        public override bool IsPlayer()
        {
            return true;
        }

        public void SendConsoleCommand(string cmd)
        {
            basePlayer.SendConsoleCommand(StringExtensions.QuoteSafe(cmd));
        }

        public void GroundTeleport(float x, float y, float z)
        {
            Teleport(x, World.GetInstance().GetGround(x, z), z);
        }
        
        public void GroundTeleport(Vector3 v3)
        {
            Teleport(v3.x, World.GetInstance().GetGround(v3.x, v3.z), v3.z);
        }

        public void Teleport(Vector3 v3)
        {
            Teleport(v3.x, v3.y, v3.z);
        }

        public static float worldSizeHalf = (float)global::World.Size / 2;
        public static Vector3[] firstLocations = new Vector3[]{
            new Vector3(worldSizeHalf, 0, worldSizeHalf),
            new Vector3(-worldSizeHalf, 0, worldSizeHalf),
            new Vector3(worldSizeHalf, 0, -worldSizeHalf),
            new Vector3(-worldSizeHalf, 0, -worldSizeHalf)
        };

        public void Teleport(float x, float y, float z)
        {  
            Vector3 firstloc = Vector3.zero;
            foreach (Vector3 v3 in firstLocations) {
                if (Vector3.Distance(Location, v3) > 1000f && Vector3.Distance(new Vector3(x, y, z), v3) > 1000f) {
                    firstloc = v3;
                }
            }

            basePlayer.transform.position = firstloc;
            //basePlayer.UpdateNetworkGroup();

            basePlayer.StartSleeping();
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);

            basePlayer.transform.position = new UnityEngine.Vector3(x, y, z);
            basePlayer.UpdateNetworkGroup();
            basePlayer.UpdatePlayerCollider(true, false);
            basePlayer.SendNetworkUpdateImmediate(false);
            basePlayer.SendFullSnapshot();
            basePlayer.inventory.SendSnapshot();

            basePlayer.CallMethod("SendNetworkUpdate_Position");
            basePlayer.ClientRPC(null, basePlayer, "StartLoading", new object[0]);
            basePlayer.Invoke("EndSleeping", 0.5f);
        }

        public bool Admin {
            get {
                return Moderator || Owner;
            }
        }

        public string AuthStatus {
            get {
                return basePlayer.net.connection.authStatus;
            }
        }

        public BasePlayer basePlayer {
            get {
                if (_basePlayer == null)
                    return BasePlayer.FindByID(GameID);
                return _basePlayer;
            }
            private set {
                _basePlayer = value;
            }
        }

        public float Health {
            get {
                return basePlayer.health;
            }
            set {
                basePlayer.health = value;
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

        public override Vector3 Location {
            get {
                return basePlayer.transform.position;
            }
            set {
                Teleport(value.x, value.y, value.z);
            }
        }

        public bool Moderator {
            get {
                return ServerUsers.Is(GameID, ServerUsers.UserGroup.Moderator);
            }
        }

        public override string Name {
            get {
                return basePlayer.displayName;
            }
        }

        public bool Offline {
            get {
                return _basePlayer == null;
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
                return Net.sv.GetAveragePing(basePlayer.net.connection);
            }
        }

        public PlayerStats Stats {
            get {
                return Server.GetInstance().serverData.Get("PlayerStats", SteamID) as PlayerStats;
            }
            set {
                Server.GetInstance().serverData.Add("PlayerStats", SteamID, value);
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
    }
}

