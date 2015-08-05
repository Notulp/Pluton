using System;

namespace Pluton.Events
{
    public class CommandEvent : CountedInstance
    {

        public readonly string[] Args;

        [Obsolete("CommandEvent.args is obsolete and will be removed, please use CommandEvent.Args", true)]
        public string[] args {
            get { return Args; }
        }

        public readonly string Cmd;

        [Obsolete("CommandEvent.cmd is obsolete and will be removed, please use CommandEvent.Cmd", true)]
        public string cmd {
            get { return Cmd; }
        }

        public string[] QuotedArgs {
            get {
                return Util.GetInstance().GetQuotedArgs(Args);
            }
        }

        [Obsolete("CommandEvent.quotedArgs is obsolete and will be removed, please use CommandEvent.QuotedArgs", true)]
        public string[] quotedArgs {
            get { return QuotedArgs; }
        }

        public string Reply;

        public readonly Player User;

        public CommandEvent(Player player, string[] command)
        {
            User = player;
            Reply = String.Format("/{0} executed!", String.Join(" ", command));
            Cmd = command[0];
            Args = new string[command.Length - 1];
            Array.Copy(command, 1, Args, 0, command.Length - 1);
        }

        public void ReplyWith(string msg)
        {
            Reply = msg;
        }
    }
}

