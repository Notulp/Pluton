using System;

namespace Pluton.Events
{
    public class CombatEntityHurtEvent : HurtEvent
    {
        public readonly Entity Victim;

        public CombatEntityHurtEvent(BaseCombatEntity combatEnt, HitInfo info)
            : base(info)
        {
            Victim = new Entity(combatEnt);
        }
    }
}

