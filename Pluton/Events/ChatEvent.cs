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
            BroadcastName = args.connection != null ? args.connection.username : Server.server_message_name;
            OriginalText = args.ArgsStr.Substring(1, args.ArgsStr.Length - 2).Replace("\\", "");
            FinalText = OriginalText;
            Reply = "chat.say was executed";
        }

        public void ReplyWith(string msg)
        {
            Reply = msg;
        }
        
        public void Cancel(string reply = "Your message was not sent")
        {
            FinalText = "";
            Reply = reply;
        }
    }
}

