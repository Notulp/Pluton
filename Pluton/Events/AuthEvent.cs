using System;
using Network;

namespace Pluton.Events
{
    public class AuthEvent
    {
        public bool approved;
        public string _reason;
        public readonly Connection con;

        public AuthEvent(Connection connection)
        {
            con = connection;
            approved = true;
        }

        public ulong GameID {
            get {
                return con.userid;
            }
        }

        public string IP {
            get {
                return con.ipaddress;
            }
        }

        public string Name {
            get {
                return con.username;
            }
        }

        public string OS {
            get {
                return con.os;
            }
        }

        public void Reject(string reason = "no reason")
        {
            approved = false;
            _reason = reason;
        }
    }
}

