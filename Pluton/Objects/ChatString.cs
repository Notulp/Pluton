using System;

namespace Pluton {
	public class ChatString {

		public readonly ConsoleSystem.Arg _arg;
		public readonly string OriginalText;
		public string BroadcastName;
		public string FinalText;
		public string ReplyWith;

		public ChatString(ConsoleSystem.Arg args) {
			_arg = args;
			if (args.FromClient)
				BroadcastName = args.connection.username;
			else
				BroadcastName = Server.server_message_name;
			OriginalText = args.ArgsStr;
			FinalText = args.ArgsStr;
			ReplyWith = "chat.say was executed";
		}
	}
}

