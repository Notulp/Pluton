using System;
using UnityEngine;

namespace Pluton {
	public class Hooks {

		#region Events

		public static event ChatDelegate OnChat;

		public static event CommandDelegate OnCommand;

		public static event PlayerConnectedDelegate OnPlayerConnected;

		public static event PlayerDisconnectedDelegate OnPlayerDisconnected;

		#endregion

		#region Handlers

		// didn't tested
		public static void Command(Player player, string[] args) {
			string cmd = args[0].Replace("/", "");
			string[] args2 = new string[args.Length - 1];
			Array.Copy(args, 1, args2, 0, args.Length - 1);
			OnCommand(player, cmd, args2);
		}

		public static void Chat(ConsoleSystem.Arg arg){
			if (arg.ArgsStr.StartsWith("\"/")) {
				Command(new Player(arg.Player()), arg.Args);
				return;
			}

			if (!chat.enabled) {
				arg.ReplyWith("Chat is disabled.");
			} else {
				BasePlayer basePlayer = ArgExtension.Player(arg);
				if (!(bool) ((UnityEngine.Object) basePlayer))
					return;

				string str = arg.GetString(0, "text");

				if (str.Length > 128)
					str = str.Substring(0, 128);

				if (chat.serverlog)
					Debug.Log((object) (basePlayer.displayName + ": " + str));

				ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(basePlayer.displayName) + " " + StringExtensions.QuoteSafe(str));
				arg.ReplyWith("chat.say was executed");
			}
			Debug.Log(arg.Player().displayName + " said: " + arg.ArgsStr);
			OnChat(arg);
		}

		public static void PlayerConnected(Network.Connection connection) {
			var player = connection.player as BasePlayer;
			Debug.Log(player.displayName + " joined the fun");
			OnPlayerConnected(new Player(player));
		}

		public static void PlayerDisconnected(BasePlayer player) {
			Debug.Log(player.displayName + " left the reality");
			OnPlayerDisconnected(new Player(player));
		}

		#endregion

		#region Delegates

		public delegate void ChatDelegate(ConsoleSystem.Arg arg);

		public delegate void CommandDelegate(Player player, string cmd, string[] args);

		public delegate void PlayerConnectedDelegate(Player player);

		public delegate void PlayerDisconnectedDelegate(Player player);

		#endregion

		public Hooks () { }
	}
}

