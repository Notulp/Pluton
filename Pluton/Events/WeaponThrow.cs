

namespace Pluton.Events
{
    public class WeaponThrow
    {
        private ThrownWeapon _w;
        private BaseEntity.RPCMessage _msg;
        private Player _player;

        public WeaponThrow(ThrownWeapon thrownWeapon, BaseEntity.RPCMessage msg)
        {
            this._msg = msg;
            this._w = thrownWeapon;
            this._player = new Player(msg.player);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._msg; }
        }

        public ThrownWeapon Weapon
        {
            get { return this._w; }
        }

        public Player Player
        {
            get { return this._player; }
        }
    }
}
