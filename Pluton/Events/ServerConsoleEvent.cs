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
            if (String.IsNullOrEmpty(rconCmd))
                return;

            _args = args;
            Args = args.Args != null ? args.Args.ToList() : new List<string>();
            cmd = rconCmd.Split(' ')[0];
            Reply = "Command not found!";
        }

        public void ReplyWith(string reply)
        {
            Reply = reply;
        }
    }
}
