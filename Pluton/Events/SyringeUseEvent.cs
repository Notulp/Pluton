namespace Pluton.Events
{
    public class SyringeUseEvent
    {
        Player _user;
        Player _receiver;
        SyringeWeapon _syringeWeapon;
        readonly bool _self;

        public SyringeUseEvent(SyringeWeapon sw,  BaseEntity.RPCMessage msg, bool isSelf)
        {
            _syringeWeapon = sw;
            _user = new Player(sw.ownerPlayer);
            _self = isSelf;

            _receiver = isSelf ? _user : new Player(BaseNetworkable.serverEntities.Find(msg.read.UInt32()) as BasePlayer);
        }

        public Player User
        {
            get { return _user; }
        }

        public Player Receiver
        {
            get { return _receiver; }
        }

        public SyringeWeapon Syringe
        {
            get { return _syringeWeapon; }
        }

        public bool IsSelfUsage()
        {
            return _self;
        }
    }
}
