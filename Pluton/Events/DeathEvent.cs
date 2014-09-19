using System;

namespace Pluton.Events {
	public class DeathEvent {

		private Player _victim;
		private HitInfo _info;

		public DeathEvent(Player player, HitInfo info) {
			_victim = player;
			_info = info;
		}

		public float DamageAmount {
			get {
				return _info.damageAmount;
			}
		}

		public string DamageType {
			get {
				return _info.damageType.ToString();
			}
		}

		public string IName {
			get {
				return _info.Initiator.name;
			}
		}

		public string IPrefab {
			get {
				return _info.Initiator.sourcePrefab;
			}
		}

		public Player Victim {
			get {
				return _victim;
			}
		}
	}
}

