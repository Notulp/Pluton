namespace Pluton.Events
{
    public class ShootEvent : CountedInstance
    {
        BaseEntity.RPCMessage _rpcMessage;
        BaseProjectile _projectile;
        Player _pl;

        public ShootEvent(BaseProjectile baseProjectile, BaseEntity.RPCMessage msg)
        {
            _pl = new Player(msg.player);
            _rpcMessage = msg;
            _projectile = baseProjectile;
        }

        public BaseProjectile BaseProjectile
        {
            get { return _projectile; }
        }

        public Player Player
        {
            get { return _pl; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _rpcMessage; }
        }
    }
}
