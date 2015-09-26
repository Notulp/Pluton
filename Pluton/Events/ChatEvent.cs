using System;

namespace Pluton.Events
{
    public class ChatEvent : CountedInstance
    {
        public readonly ConsoleSystem.Arg Arg;

        [Obsolete("ChatEvent._arg is obsolete and will be removed, please use ChatEvent.Arg", true)]
        public ConsoleSystem.Arg _arg {
            get { return Arg; }
        }

        public readonly string OriginalText;
        public readonly Player User;
        public string BroadcastName;
        public string FinalText;
        public string Reply;

        public ChatEvent(Player player, ConsoleSystem.Arg args)
        {
            User = player;
            Arg = args;
            if (args.connection != null)
                BroadcastName = args.connection.username;
            else
                BroadcastName = Server.server_message_name;
            OriginalText = args.ArgsStr.Substring(1, args.ArgsStr.Length - 2).Replace("\\", "");
            FinalText = OriginalText.Replace('<', '[').Replace('>', ']');
            Reply = "chat.say was executed";
        }

        public void Cancel(string reply = "Your message was not sent")
        {
            FinalText = "";
            Reply = reply;
        }

        public void ReplyWith(string msg) => Reply = msg;
    }
}

