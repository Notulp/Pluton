using System;
using UnityEngine;

namespace Pluton.Events
{
    public class CommandEvent : CountedInstance
    {
        public string Reply;
        public readonly string cmd;
        public readonly string[] args;
        public readonly string[] quotedArgs;
        public readonly Player User;

        public CommandEvent(Player player, string[] command)
        {
            User = player;
            Reply = String.Format("/{0} executed!", String.Join(" ", command));
            cmd = command[0];
            args = new string[command.Length - 1];
            Array.Copy(command, 1, args, 0, command.Length - 1);
            quotedArgs = Util.GetInstance().GetQuotedArgs(args);
        }

        public void ReplyWith(string msg)
        {
            Reply = msg;
        }
    }
}

