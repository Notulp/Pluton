using System;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class OfflinePlayer
    {
        public string Name;
        public string SteamID;
        public string IP;
        public string OS;
        public float X;
        public float Y;
        public float Z;
        public ulong totalTimeOnline;
        public bool Admin;

        // you shouldn't ever call this constructor manually
        public OfflinePlayer(Player player)
        {
            Name = player.Name;
            SteamID = player.SteamID;
            IP = player.IP;
            OS = player.OS;
            X = player.X;
            Y = player.Y;
            Z = player.Z;
            totalTimeOnline = (ulong)player.TimeOnline;
            Admin = player.Admin;
            /*
			 * last online
			 * kills?
			 * other stats?
			 * anything else?
			 * 
			 */
        }

        public static OfflinePlayer Get(ulong steamID)
        {
            OfflinePlayer op = Server.GetServer().OfflinePlayers[steamID];
            if (op == null) {
                Logger.LogDebug("[OfflinePlayer] Couldn't find OfflinePlayer: " + steamID.ToString());
                return null;
            }
            return op;
        }

        public static OfflinePlayer Get(string steamID)
        {
            return Get(UInt64.Parse(steamID));
        }

        public void Update(Player player)
        {
            if (Name != player.Name) {
                Logger.LogDebug("[OfflinePlayer] " + Name + " changed name to: " + player.Name);
                Name = player.Name;
            }
            IP = player.IP;
            OS = player.OS;
            X = player.X;
            Y = player.Y;
            Z = player.Z;
            Admin = player.Admin;
            totalTimeOnline += (ulong)player.TimeOnline;
        }
    }
}

