using System;

namespace Pluton.Events
{
    public class NPCHurtEvent : HurtEvent
    {
        public readonly NPC Victim;

        public NPCHurtEvent(NPC npc, HitInfo info)
            : base(info)
        {
            Victim = npc;
        }
    }
}

