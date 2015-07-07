using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pluton.Events
{
    public class RocketShootEvent
    {
        private BaseEntity.RPCMessage _rpcMessage;
        private BaseLauncher _launch;
        private Entity _ent;
        private Player _pl;

        public RocketShootEvent(BaseLauncher baseLauncher, BaseEntity.RPCMessage msg, BaseEntity baseEntity)
        {
            this._ent = new Entity(baseEntity);
            this._pl = new Player(msg.player);
            this._rpcMessage = msg;
            this._launch = baseLauncher;
        }

        public BaseLauncher BaseLauncher
        {
            get { return this._launch; }
        }

        public Player Player
        {
            get { return this._pl; }
        }

        public Entity Entity
        {
            get { return this._ent; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._rpcMessage; }
        }
    }
}
