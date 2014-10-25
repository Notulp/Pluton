using System;

namespace Pluton.Events
{
    public class PlayerTakeRadsEvent : CountedInstance
    {

		public readonly Player Victim;
		public float Amount;

		public PlayerTakeRadsEvent(Player p, float amount)
        {
			Amount = amount;
			Victim = p;
		}
	}
}

