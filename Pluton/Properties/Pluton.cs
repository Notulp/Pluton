using System;

namespace Pluton {
	[ConsoleSystem.Factory("pluton")]
	public class pluton : ConsoleSystem {
		[ConsoleSystem.Admin]
		[ConsoleSystem.Help("Helps to break basic functionality", "")]
		public static bool enabled;
		[ConsoleSystem.Admin]
		public static bool serverlog;

		static pluton() {
			pluton.enabled = true;
			pluton.serverlog = true;
		}

		public pluton() {
			base.\u002Ector();
		}

		[ConsoleSystem.Admin]
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
	}
}

