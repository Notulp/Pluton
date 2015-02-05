using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ServerConsoleEvent : CountedInstance
    {
        public readonly string cmd;
        public readonly List<object> Args;

        public bool IsServer {
            get {
                return Realm.isServer;
            }
        }

        public bool ForwardToServer {
            get {
                return Options.forwardToServer;
            }
            set {
                Options.forwardToServer = value;
            }
        }

        public bool GiveFeedback {
            get {
                return Options.giveFeedback;
            }
            set {
                Options.giveFeedback = value;
            }
        }

        public ConsoleSystem.SystemRealm Realm;
        public ConsoleSystem.RunOptions Options;

        public ServerConsoleEvent(ConsoleSystem.SystemRealm realm, ConsoleSystem.RunOptions options, string rconCmd, object[] args)
        {
            if(String.IsNullOrEmpty(rconCmd))
                return;

            this.Options = options;
            this.Realm = realm;
            this.Args = args.ToList();
            this.cmd = rconCmd;
        }
    }
}
