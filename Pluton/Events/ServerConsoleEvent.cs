using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ServerConsoleEvent : CountedInstance
    {
        public readonly ConsoleSystem.Arg _args;

        public readonly string cmd;
        public readonly List<string> Args;

        public string Reply;

        public ServerConsoleEvent(ConsoleSystem.Arg args, string rconCmd)
        {
            if(String.IsNullOrEmpty(rconCmd))
                return;

            this._args = args;
            this.Args = args.Args != null ? args.Args.ToList() : new List<string>();
            this.cmd = rconCmd.Split(' ')[0];
            this.Reply = "Command not found!";
        }

        public void ReplyWith(string reply)
        {
            Reply = reply;
        }
    }
}
