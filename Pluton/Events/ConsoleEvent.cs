using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ConsoleEvent
    {
        public readonly ConsoleSystem.Arg Internal;
        public readonly Player User;
        public readonly String Command;
        public readonly List<String> Args;

        public ConsoleEvent(ConsoleSystem.Arg arg, String rconCmd)
        {
            this.Internal = arg;
            this.User = Player.FindByGameID((arg.connection.player as BasePlayer).userID);
            this.Args = new List<String>();

            foreach (String str in rconCmd.Split(' '))
                Args.Add(str);
            
            this.Command = Args[0];
            Args.RemoveAt(0);
        }

        public void Reply(String msg)
        {
            ReflectionExtensions.CallStaticMethod(typeof(ConsoleSystem), "SendClientReply", Internal.connection, msg);
        }
    }
}
