namespace Pluton.Events
{
    public class RocketShootEvent : CountedInstance
    {
        BaseEntity.RPCMessage _rpcMessage;
        BaseLauncher _launch;
        Entity _ent;
        Player _pl;

        public RocketShootEvent(BaseLauncher baseLauncher, BaseEntity.RPCMessage msg, BaseEntity baseEntity)
        {
            _ent = new Entity(baseEntity);
            _pl = new Player(msg.player);
            _rpcMessage = msg;
            _launch = baseLauncher;
        }

        public BaseLauncher BaseLauncher
        {
            get { return _launch; }
        }

        public Player Player
        {
            get { return _pl; }
        }

        public Entity Entity
        {
            get { return _ent; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _rpcMessage; }
        }
    }
}
