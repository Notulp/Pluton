using Network;

namespace Pluton.Events
{
    public class AuthEvent : CountedInstance
    {
        public bool approved;
        public string _reason;
        public readonly Connection con;

        public AuthEvent(Connection connection)
        {
            this.con = connection;
            this.approved = true;
        }

        public Connection Connection
        {
            get { return this.con; }
        }

        public ulong GameID
        {
            get { return this.con.userid; }
        }

        public string IP
        {
            get { return this.con.ipaddress; }
        }

        public string Name
        {
            get { return this.con.username; }
        }

        public string OS
        {
            get { return this.con.os; }
        }

        public void Reject(string reason = "no reason")
        {
            this.approved = false;
            this._reason = reason;
        }
    }
}

