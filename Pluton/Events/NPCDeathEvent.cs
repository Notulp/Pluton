namespace Pluton.Events
{
    public class NPCDeathEvent : DeathEvent
    {
        public readonly NPC Victim;

        public NPCDeathEvent(NPC npc, HitInfo info)
            : base(info)
        {
            Victim = npc;
        }
    }
}

