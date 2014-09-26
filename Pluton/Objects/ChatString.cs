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
			if (args.connection != null)
				BroadcastName = args.connection.username;
			else
				BroadcastName = Server.server_message_name;
			OriginalText = args.ArgsStr.Substring(1, args.ArgsStr.Length - 2).Replace("\\", "");
			FinalText = OriginalText;
			ReplyWith = "chat.say was executed";
		}
	}
}

