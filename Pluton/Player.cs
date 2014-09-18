using System;

namespace Pluton {
	public class Player {

		public readonly BasePlayer basePlayer;

		public Player (BasePlayer player) {
			basePlayer = player;
		}

		public void Kill() {
			basePlayer.StartDead();
		}


		public string Name {
			get {
				return basePlayer.displayName;
			}
		}

		public string SteamID {
			get {
				return basePlayer.userID.ToString();
			}
		}

		public float Health {
			get {
				return basePlayer.Health();
			}
		}

		public ulong GameID {
			get {
				return basePlayer.userID;
			}
		}
	}
}

