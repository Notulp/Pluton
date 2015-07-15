namespace Pluton.Events
{
    public class WeaponThrowEvent : CountedInstance
    {
        private ThrownWeapon _thrownWeapon;
        private BaseEntity.RPCMessage _msg;
        private Player _player;

        public WeaponThrowEvent(ThrownWeapon thrownWeapon, BaseEntity.RPCMessage msg)
        {
            this._msg = msg;
            this._thrownWeapon = thrownWeapon;
            this._player = Server.GetPlayer(msg.player);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._msg; }
        }

        public ThrownWeapon Weapon
        {
            get { return this._thrownWeapon; }
        }

        public Player Player
        {
            get { return this._player; }
        }
    }
}
