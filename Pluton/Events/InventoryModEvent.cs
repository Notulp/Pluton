using System;

namespace Pluton.Events
{
    public class InventoryModEvent
    {
        private InvItem _item;
        private ItemContainer _ic;
        private Player _owner;
        private Entity _enity;

        public InventoryModEvent(ItemContainer ic, Item i)
        {
            this._item = new InvItem(i);
            this._ic = ic;
            if (ic.entityOwner != null)
            {
                this._enity = new Entity(ic.entityOwner);
            }
            if (ic.playerOwner != null)
            {
                this._owner = new Player(ic.playerOwner);
            }
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public ItemContainer ItemContainer
        {
            get { return this._ic; }
        }

        public Player Player
        {
            get { return this._owner; }
        }

        public Entity Entity
        {
            get { return this._enity; }
        }
    }
}