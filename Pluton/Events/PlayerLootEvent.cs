using System;

namespace Pluton.Events
{
    public class PlayerLootEvent : LootEvent
    {
        public readonly Player Target;

        public PlayerLootEvent(PlayerLoot pl, Player looter, Player looted)
            : base(pl, looter)
        {
            Target = looted;
        }
    }
}

