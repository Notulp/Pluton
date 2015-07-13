namespace Pluton.Events
{
    public class ItemPickupEvent : CountedInstance
    {
        InvItem _item;
        Player _player;
        BaseEntity.RPCMessage _msg;
        CollectibleEntity _ent;

        public ItemPickupEvent(CollectibleEntity ce, BaseEntity.RPCMessage msg, Item i)
        {
            _ent = ce;
            _msg = msg;
            _player = new Player(msg.player);
            _item = new InvItem(i);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _msg; }
        }

        public CollectibleEntity Entity
        {
            get { return _ent; }
        }

        public InvItem Item
        {
            get { return _item; }
        }

        public Player Player
        {
            get { return _player; }
        }
    }
}
