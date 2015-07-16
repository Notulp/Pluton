namespace Pluton.Events
{
    public class ShootEvent : CountedInstance
    {
        private BaseEntity.RPCMessage _rpcMessage;
        private BaseProjectile _projectile;
        private Player _player;

        public ShootEvent(BaseProjectile baseProjectile, BaseEntity.RPCMessage msg)
        {
            _player = Server.GetPlayer(msg.player);
            _rpcMessage = msg;
            _projectile = baseProjectile;
        }

        public BaseProjectile BaseProjectile
        {
            get { return _projectile; }
        }

        public Player Player
        {
            get { return _player; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _rpcMessage; }
        }
    }
}
