using System;

namespace Pluton.Events {
	public class PlayerDeathEvent : DeathEvent {

		private Player _victim;

		public PlayerDeathEvent(Player player, HitInfo info) : base(info) {
			_victim = player;
		}

		public Player Victim {
			get {
				return _victim;
			}
		}
	}
}

