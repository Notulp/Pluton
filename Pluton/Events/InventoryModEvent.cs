namespace Pluton.Events
{
    public class InventoryModEvent
    {
        InvItem _item;
        ItemContainer _ic;
        Player _owner;
        Entity _enity;

        public InventoryModEvent(ItemContainer ic, Item i)
        {
            _item = new InvItem(i);
            _ic = ic;
            if (ic.entityOwner != null)
            {
                _enity = new Entity(ic.entityOwner);
            }
            if (ic.playerOwner != null)
            {
                _owner = new Player(ic.playerOwner);
            }
        }

        public InvItem Item
        {
            get { return _item; }
        }

        public ItemContainer ItemContainer
        {
            get { return _ic; }
        }

        public Player Player
        {
            get { return _owner; }
        }

        public Entity Entity
        {
            get { return _enity; }
        }
    }
}