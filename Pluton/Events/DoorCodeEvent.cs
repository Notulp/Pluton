using System.Collections.Generic;

namespace Pluton.Events
{
    public class DoorCodeEvent : CountedInstance
    {
        public Player Player;
        public CodeLock codeLock;

        public bool allowed = true;
        public bool forceAllow = false;

        private string entered;

        public DoorCodeEvent(CodeLock doorLock, BasePlayer player, string entered)
        {
            this.codeLock = doorLock;
            this.entered = entered;
            this.Player = Server.GetPlayer(player);
        }

        public void Whitelist()
        {
            List<ulong> whitelist = new List<ulong>();
            whitelist = (List<ulong>)codeLock.GetFieldValue("whitelistPlayers");
            whitelist.Add(Player.GameID);
            codeLock.SetFieldValue("whitelistPlayers", whitelist);
        }

        public void ClearWhitelist()
        {
            codeLock.SetFieldValue("whitelistPlayers", new List<ulong>());
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

        public void Deny()
        {
            allowed = false;
        }

        public void Allow()
        {
            forceAllow = true;
        }

        public string Code
        {
            get
            {
                return (string)codeLock.GetFieldValue("code");
            }
            set
            {
                int nothing;
                if (value.Length == 4 && int.TryParse(value, out nothing))
                {
                    codeLock.SetFieldValue("code", value);
                }
            }
        }

        public string Entered
        {
            get
            {
                return entered;
            }
            set
            {
                int nothing;
                if (value.Length == 4 && int.TryParse(value, out nothing))
                {
                    entered = value;
                }
            }
        }

        public bool IsCorrect()
        {
            return entered == (string)codeLock.GetFieldValue("code");
        }
    }
}

