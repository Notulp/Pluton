using System;

namespace Pluton.Events
{
    public class BuildingHurtEvent : HurtEvent
    {
        public readonly BuildingPart Victim;

        public BuildingHurtEvent(BuildingPart bp, HitInfo info)
            : base(info)
        {
            Victim = bp;
        }
    }
}

