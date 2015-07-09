using System;

namespace Pluton.Events
{
    public class SyringeUseEvent
    {
        private Player _user;
        private Player _receiver;
        private SyringeWeapon _syringeWeapon;
        private bool _self;

        public SyringeUseEvent(SyringeWeapon sw,  BaseEntity.RPCMessage msg, bool isSelf)
        {
            _syringeWeapon = sw;
            _user = new Player(sw.ownerPlayer);
            _self = isSelf;

            if (isSelf)
                _receiver = _user;
            else
                _receiver = new Player(BaseNetworkable.serverEntities.Find(msg.read.UInt32()) as BasePlayer);
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
