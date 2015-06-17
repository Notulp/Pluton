using System;

namespace Pluton.Events
{
    public class PlayerTakedmgEvent : CountedInstance
    {

        public readonly Player Victim;
        public float Amount;
        public Rust.DamageType Type;

        public PlayerTakedmgEvent(Player p, float amount, Rust.DamageType type)
        {
            Type = type;
            Amount = amount;
            Victim = p;
        }
    }
}

