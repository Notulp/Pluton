using System;

namespace Pluton.Events {
	public class GatherEvent {

		private HitInfo _info;
		private BaseResource _res;

		public GatherEvent(HitInfo info, BaseResource res) {
			_info = info;
			_res = res;
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

		public float Health {
			get {
				return _res.health;
			}
		}

		public string Prefab {
			get {
				return _res.sourcePrefab;
			}
		}

		public int Stage {
			get {
				return _res.stage;
			}
		}

		public string WeaponName {
			get {
				return _info.Weapon.info.displayname;
			}
		}
	}
}

