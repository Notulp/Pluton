namespace Pluton.Events
{
    public class RocketShootEvent : CountedInstance
    {
        private BaseEntity.RPCMessage _msg;
        private BaseLauncher _launch;
        private Entity _entity;
        private Player _player;

        public RocketShootEvent(BaseLauncher baseLauncher, BaseEntity.RPCMessage msg, BaseEntity baseEntity)
        {
            _entity = new Entity(baseEntity);
            _player = Server.GetPlayer(msg.player);
            _msg = msg;
            _launch = baseLauncher;
        }

        public BaseLauncher BaseLauncher
        {
            get { return _launch; }
        }

        public Player Player
        {
            get { return _player; }
        }

        public Entity Entity
        {
            get { return _entity; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _msg; }
        }
    }
}
