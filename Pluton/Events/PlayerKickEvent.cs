using System;
using Network;

namespace Pluton
{
    public class PlayerKickEvent : CountedInstance
    {
        public bool Kick = false;
        public string Reason;

        public Connection Connection;

        public PlayerKickEvent(Connection connection, string reason)
        {
            Connection = connection;
            Reason = reason;
        }
    }
}

