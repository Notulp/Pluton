using System;

namespace Pluton.Events
{
    public class LootEvent
    {
        public readonly Player Looter;
        public readonly PlayerLoot pLoot;

        public LootEvent(PlayerLoot pl, Player looter)
        {
            Looter = looter;
            pLoot = pl;
        }
    }
}

