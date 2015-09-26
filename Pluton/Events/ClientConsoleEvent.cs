using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ClientConsoleEvent : CountedInstance
    {
        public readonly ConsoleSystem.Arg Internal;
        public readonly Player User;

        public readonly string Cmd;

        [Obsolete("ClientConsoleEvent.cmd is obsolete and will be removed, please use ClientConsoleEvent.Cmd", true)]
        public string cmd {
            get { return Cmd; }
        }

        public readonly List<string> Args;
        public string Reply;

        public ClientConsoleEvent(ConsoleSystem.Arg arg, string rconCmd)
        {
            Internal = arg;
            User = Server.GetPlayer((BasePlayer)arg.connection.player);
            Args = new List<string>();

            Reply = "Command not found!";

            if (String.IsNullOrEmpty(rconCmd))
                return;

            foreach (string str in rconCmd.Split(' '))
                Args.Add(str);

            Cmd = Args[0];
            Args.RemoveAt(0);
        }

        public void ReplyWith(string msg) => Reply = msg;
    }
}
