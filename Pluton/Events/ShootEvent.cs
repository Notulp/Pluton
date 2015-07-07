namespace Pluton.Events
{
    public class ShootEvent : CountedInstance
    {
        private BaseEntity.RPCMessage _rpcMessage;
        private BaseProjectile _projectile;
        private Player _pl;

        public ShootEvent(BaseProjectile baseProjectile, BaseEntity.RPCMessage msg)
        {
            this._pl = new Player(msg.player);
            this._rpcMessage = msg;
            this._projectile = baseProjectile;
        }

        public BaseProjectile BaseProjectile
        {
            get { return this._projectile; }
        }

        public Player Player
        {
            get { return this._pl; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._rpcMessage; }
        }
    }
}
