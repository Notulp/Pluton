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
                _gatherer = Server.GetPlayer((BasePlayer)to);
                _resource = new Entity(from);
                _resourceDispenser = dispenser;
                _itemAmount = itemAmt;
                _amount = (int)(amount * World.GetInstance().ResourceGatherMultiplier);
            }
        }

        public ResourceDispenser ResourceDispenser {
            get { return _resourceDispenser; }
        }

        public Player Gatherer {
            get { return _gatherer; }
        }

        public Entity Resource {
            get { return _resource; }
        }

        public ItemAmount ItemAmount {
            get { return _itemAmount; }
        }

        public int Amount {
            get { return _amount; }
            set { _amount = value; }
        }
    }
}

