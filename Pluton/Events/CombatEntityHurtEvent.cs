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
                this.Victim = new BuildingPart(block);
            else
                this.Victim = new Entity(combatEnt);
        }
    }
}

