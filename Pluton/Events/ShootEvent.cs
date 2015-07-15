namespace Pluton.Events
{
    public class ShootEvent : CountedInstance
    {
        private BaseEntity.RPCMessage _rpcMessage;
        private BaseProjectile _projectile;
        private Player _player;

        public ShootEvent(BaseProjectile baseProjectile, BaseEntity.RPCMessage msg)
        {
            this._player = Server.GetPlayer(msg.player);
            this._rpcMessage = msg;
            this._projectile = baseProjectile;
        }

        public BaseProjectile BaseProjectile
        {
            get { return this._projectile; }
        }

        public Player Player
        {
            get { return this._player; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._rpcMessage; }
        }
    }
}
