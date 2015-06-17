using System;

namespace Pluton.Events
{
    public class ChatEvent : CountedInstance
    {
        public readonly ConsoleSystem.Arg _arg;
        public readonly string OriginalText;
        public readonly Player User;
        public string BroadcastName;
        public string FinalText;
        public string Reply;

        public ChatEvent(Player player, ConsoleSystem.Arg args)
        {
            User = player;
            _arg = args;
            if (args.connection != null)
                BroadcastName = args.connection.username;
            else
                BroadcastName = Server.server_message_name;
            OriginalText = args.ArgsStr.Substring(1, args.ArgsStr.Length - 2).Replace("\\", "");
            FinalText = OriginalText;
            Reply = "chat.say was executed";
        }

        public void ReplyWith(string msg)
        {
            Reply = msg;
        }
    }
}
