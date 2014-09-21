using System;

namespace Pluton.Events {
	public class DeathEvent {

		private HitInfo _info;
		public bool dropLoot;

		public DeathEvent(HitInfo info) {
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

		public BaseEntity Attacker{
			get {
				return _info.Initiator;
			}
		}
	}
}

