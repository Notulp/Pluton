using System;

namespace Pluton.Events
{
    public class MetabolismTickEvent : CountedInstance
    {
        public readonly Player Victim;

        public float CurrentTemperature = 0.0f;
        public float FutureTemperature = 0.0f;

        public float debug = 0.0f;
        public float debug2 = 0.0f;

        public float CaloriesHealthChange = 0.0f;
        public float HydrationHealthChange = 0.0f;
        public float CaloriesChange = 0.0f;
        public float HydrationChange = 0.0f;
        public float HeartrateValue = 0.0f;
        public float OxygenValue = 0.0f;
        public float WetnessValue = 0.0f;
        public float BleedingValue = 0.0f;
        public float PoisonValue = 0.0f;
        public float RadiationValue = 0.0f;
        public bool PreventDamage = false;
        

        public MetabolismTickEvent(BasePlayer player)
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

