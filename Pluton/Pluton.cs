using System;

namespace Pluton {
	[ConsoleSystem.Factory("pluton")]
	public class pluton : ConsoleSystem {
		[ConsoleSystem.Admin]
		[ConsoleSystem.Help("Helps to break basic functionality", "")]
		public static bool enabled;

		static pluton() {
			pluton.enabled = true;
		}

		public pluton() {
			base.\u002Ector();
		}

		[ConsoleSystem.User]
		[ConsoleSystem.Help("pluton.login <rcon.password>", "")]
		public static void login(ConsoleSystem.Arg arg) {
			if (arg.connection != null && arg.ArgsStr == rcon.password) {
				ServerUsers.Set(arg.connection.userid, ServerUsers.UserGroup.Moderator, arg.connection.username, "Console login!");
				ServerUsers.Save();
				arg.ReplyWith("You are a moderator now!");
			}
		}

		[ConsoleSystem.Admin]
		[ConsoleSystem.Help("pluton.reload <optional = plugin name>", "")]
		public static void reload(ConsoleSystem.Arg arg) {
			if (PluginLoader.Plugins.ContainsKey (arg.ArgsStr)) {
				PluginLoader.GetInstance().ReloadPlugin(arg.ArgsStr);
				arg.ReplyWith(String.Format("{0} plugin reloaded!", arg.ArgsStr));
			} else if (arg.ArgsStr == "") {
				PluginLoader.GetInstance().ReloadPlugins();
				arg.ReplyWith("Pluton reloaded!");
			} else {
				arg.ReplyWith(String.Format("Couldn't find plugin: {0}!", arg.ArgsStr));
			}
		}

		[ConsoleSystem.Admin]
		[ConsoleSystem.Help("manually saves stats, server", "")]
		public static void saveall(ConsoleSystem.Arg arg) {
			Bootstrap.SaveAll();
			arg.ReplyWith("Everything saved!");
		}
	}
}

