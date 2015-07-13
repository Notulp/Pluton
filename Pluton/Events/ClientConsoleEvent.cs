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
            Internal = arg;
            User = Player.FindByGameID((arg.connection.player as BasePlayer).userID);
            Args = new List<string>();

            Reply = "Command not found!";

            if (String.IsNullOrEmpty(rconCmd))
                return;

            foreach (string str in rconCmd.Split(' '))
                Args.Add(str);

            cmd = Args[0];
            Args.RemoveAt(0);
        }

        public void ReplyWith(string msg)
        {
            Reply = msg;
        }
    }
}
