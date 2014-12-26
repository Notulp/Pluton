using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ClientConsoleEvent : CountedInstance
    {
        public readonly ConsoleSystem.Arg Internal;
        public readonly Player User;
        public readonly String cmd;
        public readonly List<String> Args;
        public String Reply;

        public ClientConsoleEvent(ConsoleSystem.Arg arg, String rconCmd)
        {
            this.Internal = arg;
            this.User = Player.FindByGameID((arg.connection.player as BasePlayer).userID);
            this.Args = new List<String>();

            Reply = String.Format("{0} was executed from console!", rconCmd);

            if(String.IsNullOrEmpty(rconCmd))
                return;

            foreach (String str in rconCmd.Split(' '))
                Args.Add(str);

            this.cmd = Args[0];
            Args.RemoveAt(0);
        }

        public void ReplyWith(String msg)
        {
            Reply = msg;
        }
    }
}
