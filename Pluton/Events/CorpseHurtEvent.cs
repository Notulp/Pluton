using System;

namespace Pluton.Events
{
	public class CorpseHurtEvent : HurtEvent
    {

		public readonly BaseCorpse corpse;

		public CorpseHurtEvent (BaseCorpse c, HitInfo info)
            :base(info)
        {
			corpse = c;
		}
	}
}

