namespace Pluton.Events
{
    public class WeaponThrowEvent : CountedInstance
    {
        private ThrownWeapon _thrownWeapon;
        private BaseEntity.RPCMessage _msg;
        private Player _player;

        public WeaponThrowEvent(ThrownWeapon thrownWeapon, BaseEntity.RPCMessage msg)
        {
            _msg = msg;
            _thrownWeapon = thrownWeapon;
            _player = Server.GetPlayer(msg.player);
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return _msg; }
        }

        public ThrownWeapon Weapon
        {
            get { return _thrownWeapon; }
        }

        public Player Player
        {
            get { return _player; }
        }
    }
}
