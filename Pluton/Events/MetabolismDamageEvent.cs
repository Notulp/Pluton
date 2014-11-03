using System;

namespace Pluton.Events
{
    public class MetabolismDamageEvent : CountedInstance
    {
        public readonly Player Victim;

        public float HungerDamage = 0.0f;
        public float ThirstDamage = 0.0f;
        public float ColdDamage = 0.0f;
        public float HeatDamage = 0.0f;
        public float DrownedDamage = 0.0f;
        public float BleedingDamage = 0.0f;
        public float PoisonDamage = 0.0f;
        public float RadiationDamage = 0.0f;

        public MetabolismDamageEvent(BasePlayer player)
        {
            Victim = Server.GetPlayer(player);
        }

        public PlayerMetabolism metabolism
        {
            get
            {
                return Victim.basePlayer.metabolism;
            }
        }

    }
}

