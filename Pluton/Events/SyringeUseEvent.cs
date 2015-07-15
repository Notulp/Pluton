namespace Pluton.Events
{
    public class SyringeUseEvent : CountedInstance
    {
        private Player _user;
        private Player _receiver;
        private SyringeWeapon _syringeWeapon;
        private bool _self;

        public SyringeUseEvent(SyringeWeapon sw,  BaseEntity.RPCMessage msg, bool isSelf)
        {
            this._syringeWeapon = sw;
            this._user = Server.GetPlayer(sw.ownerPlayer);
            this._self = isSelf;

            if (isSelf)
                this._receiver = _user;
            else
                this._receiver = new Player(BaseNetworkable.serverEntities.Find(msg.read.UInt32()) as BasePlayer);
        }

        public Player User
        {
            get { return this._user; }
        }

        public Player Receiver
        {
            get { return this._receiver; }
        }

        public SyringeWeapon Syringe
        {
            get { return this._syringeWeapon; }
        }

        public bool IsSelfUsage()
        {
            return this._self;
        }
    }
}
