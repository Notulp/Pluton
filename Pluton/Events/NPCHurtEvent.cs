using System;

namespace Pluton.Events {
	public class NPCHurtEvent : HurtEvent {

		private NPC _victim;
		private HitInfo _info;

		public NPCHurtEvent(NPC npc, HitInfo info) : base(info){
			_victim = npc;
			_info = info;
		}

		public NPC Victim {
			get {
				return _victim;
			}
		}
	}
}

