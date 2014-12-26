using System;

namespace Pluton.Events
{
    public class LootEvent : CountedInstance
    {
        public bool Cancel = false;
        public string cancelReason = "A plugin stops you from looting that!";
        public readonly Player Looter;
        public readonly PlayerLoot pLoot;

        public LootEvent(PlayerLoot pl, Player looter)
        {
            Looter = looter;
            pLoot = pl;
        }
    }
}

