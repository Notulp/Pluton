using System;

namespace Pluton.Events
{
    public class DoorCodeEvent
    {
        public CodeLock codeLock;
        public BaseEntity.RPCMessage msg;

        public readonly string doorCode;

        public DoorCodeEvent(CodeLock doorLock, BaseEntity.RPCMessage msg)
        {
            this.msg = msg;
            codeLock = doorLock;
            doorCode = (string)doorLock.GetFieldValue("code");
        }

        public void Deny()
        {
            if (doorCode == "0000")
                msg.dataString = "0001";
            else
                msg.dataString = "0000";
        }

        public string Code {
            get {
                return msg.dataString;
            }
            set {
                msg.dataString = value;
            }
        }

        public Player Player {
            get {
                return Server.GetPlayer(msg.player);
            }
        }

        public bool IsCorrect()
        {
            return msg.dataString == doorCode;
        }
    }
}

