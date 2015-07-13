namespace Pluton.Events
{
    public class GatherEvent : CountedInstance
    {
        public ResourceDispenser resourceDispenser;
        public Player Gatherer;
        public Entity Resource;
        public ItemAmount ItemAmount;
        public int Amount;
        //public readonly int origAmount;

        public GatherEvent(ResourceDispenser dispenser, BaseEntity from, BaseEntity to, ItemAmount itemAmt, int amount)
        {
            var basePlayer = to as BasePlayer;
            if (basePlayer != null) {
                Gatherer = Server.GetPlayer(basePlayer);
                Resource = new Entity(from);
                resourceDispenser = dispenser;
                ItemAmount = itemAmt;
                Amount = (int)(amount * World.GetInstance().ResourceGatherMultiplier);
            }
        }
    }
}

