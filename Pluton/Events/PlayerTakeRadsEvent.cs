using System;

namespace Pluton.Events
{
    public class PlayerTakeRadsEvent : CountedInstance
    {

        public readonly Player Victim;
        public readonly float RadAmount;
        public readonly float Current;
        public float Next;

        public PlayerTakeRadsEvent(Player p, float current, float amount)
        {
            RadAmount = amount;
            Current = current;
            Next = UnityEngine.Mathf.Max(amount, current);
			Victim = p;
		}
	}
}

