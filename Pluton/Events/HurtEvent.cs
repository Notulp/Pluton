using System;

namespace Pluton.Events {
	public class HurtEvent {

		private HitInfo _info;

		public HurtEvent(HitInfo info) {
			_info = info;
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
	}
}

