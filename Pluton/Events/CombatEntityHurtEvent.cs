namespace Pluton.Events
{
    public class CombatEntityHurtEvent : HurtEvent
    {
        public readonly Entity Victim;

        public CombatEntityHurtEvent(BaseCombatEntity combatEnt, HitInfo info)
            : base(info)
        {
            var block = combatEnt.GetComponent<BuildingBlock>();

            if (block != null)
                Victim = new BuildingPart(block);
            else
                Victim = new Entity(combatEnt);
        }
    }
}

