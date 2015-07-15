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
            this._entity = new Entity(baseEntity);
            this._player = Server.GetPlayer(msg.player);
            this._msg = msg;
            this._launch = baseLauncher;
        }

        public BaseLauncher BaseLauncher
        {
            get { return this._launch; }
        }

        public Player Player
        {
            get { return this._player; }
        }

        public Entity Entity
        {
            get { return this._entity; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._msg; }
        }
    }
}
