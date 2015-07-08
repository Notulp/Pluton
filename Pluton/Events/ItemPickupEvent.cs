using System;

namespace Pluton.Events
{
    public class ItemPickupEvent : CountedInstance
    {
        private InvItem _item;
        private Player _player;
        private BaseEntity.RPCMessage _msg;
        private CollectibleEntity _ent;

        public ItemPickupEvent(CollectibleEntity ce, BaseEntity.RPCMessage msg, Item i)
        {
            this._ent = ce;
            this._msg = msg;
            this._player = new Player(msg.player);
            this._item = new InvItem(i);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._msg; }
        }

        public CollectibleEntity Entity
        {
            get { return this._ent; }
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public Player Player
        {
            get { return this._player; }
        }
    }
}
