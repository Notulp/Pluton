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
            this.User = player;
            this._arg = args;
            if (args.connection != null)
                this.BroadcastName = args.connection.username;
            else
                this.BroadcastName = Server.server_message_name;
            this.OriginalText = args.ArgsStr.Substring(1, args.ArgsStr.Length - 2).Replace("\\", "");
            this.FinalText = OriginalText;
            this.Reply = "chat.say was executed";
        }

        public void ReplyWith(string msg)
        {
            this.Reply = msg;
        }
        
        public void Cancel(string reply = "Your message was not sent")
        {
            this.FinalText = "";
            this.Reply = reply;
        }
    }
}

