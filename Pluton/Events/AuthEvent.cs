using System;
using Network;

namespace Pluton.Events
{
    public class AuthEvent : CountedInstance
    {
        public bool Approved;
        [Obsolete("AuthEvent.approved is obsolete and will be removed, please use AuthEvent.Approved", true)]
        public bool approved {
            get { return Approved; }
            set { Approved = value; }
        }

        public readonly Connection Connection;
        [Obsolete("AuthEvent.con is obsolete and will be removed, please use AuthEvent.Connection", true)]
        public Connection con {
            get { return Connection; }
        }

        public ulong GameID {
            get { return Connection.userid; }
        }

        public string IP {
            get { return Connection.ipaddress; }
        }

        public string Name {
            get { return Connection.username; }
        }

        public string OS {
            get { return Connection.os; }
        }

        public string Reason;
        [Obsolete("AuthEvent._reason is obsolete and will be removed, please use AuthEvent.Reason", true)]
        public string _reason {
            get { return Reason; }
            set { Reason = value; }
        }

        public AuthEvent(Connection connection)
        {
            Connection = connection;
            Approved = true;
        }

        public void Reject(string reason = "no reason")
        {
            Approved = false;
            Reason = reason;
        }
    }
}

