namespace Pluton.Events
{
    public class ItemPickupEvent : CountedInstance
    {
        private InvItem _item;
        private Player _player;
        private BaseEntity.RPCMessage _msg;
        private CollectibleEntity _entity;

        public ItemPickupEvent(CollectibleEntity ce, BaseEntity.RPCMessage msg, Item i)
        {
            _entity = ce;
            _msg = msg;
            _player = Server.GetPlayer(msg.player);
            _item = new InvItem(i);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _msg; }
        }

        public CollectibleEntity Entity
        {
            get { return _entity; }
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
