namespace Pluton.Events
{
    public class InventoryModEvent : CountedInstance
    {
        private InvItem _item;
        private ItemContainer _itemContainer;
        private Player _owner;
        private Entity _entity;

        public InventoryModEvent(ItemContainer ic, Item i)
        {
            this._item = new InvItem(i);
            this._itemContainer = ic;

            if (ic.entityOwner != null)
                this._entity = new Entity(ic.entityOwner);
            if (ic.playerOwner != null)
                this._owner = new Player(ic.playerOwner);
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public ItemContainer ItemContainer
        {
            get { return this._itemContainer; }
        }

        public Player Player
        {
            get { return this._owner; }
        }

        public Entity Entity
        {
            get { return this._entity; }
        }
    }
}