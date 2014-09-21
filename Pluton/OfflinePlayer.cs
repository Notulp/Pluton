using System;
using UnityEngine;

namespace Pluton {
	public class OfflinePlayer {

		public string Name;
		public string SteamID;
		public string IP;
		public string OS;
		public float X;
		public float Y;
		public float Z;
		public ulong totalTimeOnline;
		public bool Admin;

		public OfflinePlayer(Player player) {
			Name = player.Name;
			SteamID = player.SteamID;
			IP = player.IP;
			OS = player.OS;
			X = player.X;
			Y = player.Y;
			Z = player.Z;
			totalTimeOnline = (ulong)player.TimeOnline;
			Admin = player.Admin;
		}

		public OfflinePlayer(string str) {
			var arr = str.Split(new char[] {'|'});
			if (!arr[0].StartsWith("T:OP")) {
				Logger.LogWarning("[OfflinePlayer] Invalid string: " + str);
				return;
			}
			try {
				Name = arr[1].Replace("N:", "");
				SteamID = arr[2].Replace("SID:", "");
				IP = arr[3].Replace("IP:", "");
				OS = arr[4].Replace("OS:", "");
				X = Single.Parse(arr[5].Replace("X:", ""));
				Y = Single.Parse(arr[6].Replace("Y:", ""));
				Z = Single.Parse(arr[7].Replace("Z:", ""));
				totalTimeOnline = UInt64.Parse(arr[8].Replace("TTO:", ""));
				Admin = arr[9].Replace("A:", "")=="true"?true:false;
			} catch (Exception ex) {
				Logger.LogException(ex);
			}
		}

		public static OfflinePlayer Get(ulong steamID) {
			var op = Server.GetServer().OfflinePlayers[steamID];
			if (op == null) {
				Logger.LogDebug("Couldn't find OfflinePlayer: " + steamID.ToString());
				return null;
			}
			return op;
		}

		public static OfflinePlayer Get(string steamID) {
			return Get(UInt64.Parse(steamID));
		}

		public void Update(Player player) {
			if (Name != player.Name) {
				Logger.LogDebug(Name + " changed name to: " + player.Name);
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

		public override string ToString() {
			return String.Format("T:OP|N:{0}|SID:{1}|IP:{2}|OS:{3}|X:{4}|Y:{5}|Z:{6}|TTO:{7}|A:",
				new object[]{Name, SteamID, IP, OS,
					X.ToString("G9"), Y.ToString("G9"), Z.ToString("G9"),
					totalTimeOnline, Admin?"true":"false"});
		}
	}
}

