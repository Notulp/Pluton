using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class DoorCodeEvent : CountedInstance
    {
        public Player Player;
        public CodeLock codeLock;

        public bool allowed = true;
        public bool forceAllow = false;

        private string _entered;

        public string Code {
            get {
                return (string)codeLock.GetFieldValue("code");
            }
            set {
                int code;
                if (value.Length == 4 && Int32.TryParse(value, out code)) {
                    codeLock.SetFieldValue("code", code);
                }
            }
        }

        public string Entered {
            get {
                return _entered;
            }
            set {
                int code;
                if (value.Length == 4 && Int32.TryParse(value, out code)) {
                    _entered = value;
                }
            }
        }

        public DoorCodeEvent(CodeLock doorLock, BasePlayer player, string entered)
        {
            codeLock = doorLock;
            _entered = entered;
            Player = Server.GetPlayer(player);
        }

        public void Allow()
        {
            forceAllow = true;
        }

        public void ClearWhitelist()
        {
            codeLock.SetFieldValue("whitelistPlayers", new List<ulong>());
        }

        public void Deny()
        {
            allowed = false;
        }

        public bool IsCorrect()
        {
            return _entered == Code;
        }

        public void RemoveCode()
        {
            codeLock.SetFieldValue("code", "");
            codeLock.SetFieldValue("hasCode", false);
            codeLock.SetFlag(BaseEntity.Flags.Locked, false);
            Allow();
        }

        public void ResetLock()
        {
            codeLock.SetFieldValue("code", "");
            codeLock.SetFieldValue("hasCode", false);
            codeLock.SetFlag(BaseEntity.Flags.Locked, false);
            codeLock.SetFieldValue("whitelistPlayers", new List<ulong>());
        }

        public void Whitelist()
        {
            List<ulong> whitelist = new List<ulong>();
            whitelist = (List<ulong>)codeLock.GetFieldValue("whitelistPlayers");
            whitelist.Add(Player.GameID);
            codeLock.SetFieldValue("whitelistPlayers", whitelist);
        }
    }
}

