using System;

namespace Pluton.Events
{
    public class CombatEntityHurtEvent : HurtEvent
    {
        public readonly CombatEntity Victim;

        public CombatEntityHurtEvent(BaseCombatEntity combatEnt, HitInfo info)
            : base(info)
        {
            Victim = new CombatEntity(combatEnt);
        }
    }
}

