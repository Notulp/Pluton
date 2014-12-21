using System;

namespace Pluton
{
    public class CombatEntity : CountedInstance
    {
        [NonSerialized]
        public readonly BaseCombatEntity baseCombatEntity;

        public CombatEntity(BaseCombatEntity combatEnt)
        {
            baseCombatEntity = combatEnt;
        }

        public string Name {
            get {
                if (baseCombatEntity.name == "player/player")
                    return (baseCombatEntity as BasePlayer).displayName;
                return baseCombatEntity.name;
            }
        }
    }
}

