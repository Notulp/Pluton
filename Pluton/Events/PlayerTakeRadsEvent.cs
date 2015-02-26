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
            float next = UnityEngine.Mathf.Clamp(amount, p.basePlayer.metabolism.radiation_level.min, p.basePlayer.metabolism.radiation_level.max);
            Next = next <= current ? current : next;
			Victim = p;
		}
	}
}

