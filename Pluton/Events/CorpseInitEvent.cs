using System;

namespace Pluton.Events
{
	public class CorpseInitEvent
    {

		public readonly BaseCorpse Corpse;
		public readonly Entity Entity;

		public CorpseInitEvent(BaseCorpse c, BaseEntity p)
        {
			Corpse = c;
			Entity = new Entity(p);
		}
	}
}

