namespace Pluton.Events
{
    public class NPCDeathEvent : HurtEvent
    {
        public readonly NPC Victim;

        public NPCDeathEvent(NPC npc, HitInfo info)
            : base(info)
        {
            this.Victim = npc;
        }
    }
}

