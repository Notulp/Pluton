namespace Pluton.Events
{
    public class CombatEntityHurtEvent : HurtEvent
    {
        public readonly Entity Victim;

        public CombatEntityHurtEvent(BaseCombatEntity combatEnt, HitInfo info)
            : base(info)
        {
            var block = combatEnt.GetComponent<BuildingBlock>();

            Victim = block != null ? new BuildingPart(block) : new Entity(combatEnt);
        }
    }
}

