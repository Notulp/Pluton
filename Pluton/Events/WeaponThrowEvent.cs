namespace Pluton.Events
{
    public class WeaponThrowEvent : CountedInstance
    {
        ThrownWeapon _w;
        BaseEntity.RPCMessage _msg;
        readonly Player _player;

        public WeaponThrowEvent(ThrownWeapon thrownWeapon, BaseEntity.RPCMessage msg)
        {
            _msg = msg;
            _w = thrownWeapon;
            _player = new Player(msg.player);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _msg; }
        }

        public ThrownWeapon Weapon
        {
            get { return _w; }
        }

        public Player Player
        {
            get { return _player; }
        }
    }
}
