namespace Pluton.Events
{
    public class GatherEvent : CountedInstance
    {
        private ResourceDispenser _resourceDispenser;
        private Player _gatherer;
        private Entity _resource;
        private ItemAmount _itemAmount;
        private int _amount;

        public GatherEvent(ResourceDispenser dispenser, BaseEntity from, BaseEntity to, ItemAmount itemAmt, int amount)
        {
            if (to is BasePlayer) {
                this._gatherer = Server.GetPlayer((BasePlayer) to);
                this._resource = new Entity(from);
                this._resourceDispenser = dispenser;
                this._itemAmount = itemAmt;
                this._amount = (int)(amount * World.GetInstance().ResourceGatherMultiplier);
            }
        }

        public ResourceDispenser ResourceDispenser
        {
            get { return this._resourceDispenser; }
        }

        public Player Gatherer
        {
            get { return this._gatherer; }
        }

        public Entity Resource
        {
            get { return this._resource; }
        }

        public ItemAmount ItemAmount
        {
            get { return this._itemAmount; }
        }

        public int Amount
        {
            get { return this._amount; }
        }
    }
}

