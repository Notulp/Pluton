using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ClientConsoleEvent : CountedInstance
    {
        public readonly ConsoleSystem.Arg Internal;
        public readonly Player User;
        public readonly string cmd;
        public readonly List<string> Args;
        public string Reply;

        public ClientConsoleEvent(ConsoleSystem.Arg arg, string rconCmd)
        {
            this.Internal = arg;
            this.User = Server.GetPlayer((BasePlayer) arg.connection.player);
            this.Args = new List<string>();

            this.Reply = "Command not found!";

            if (String.IsNullOrEmpty(rconCmd))
                return;

            foreach (string str in rconCmd.Split(' '))
                this.Args.Add(str);

            this.cmd = Args[0];
            this.Args.RemoveAt(0);
        }

        public void ReplyWith(string msg)
        {
            this.Reply = msg;
        }
    }
}
