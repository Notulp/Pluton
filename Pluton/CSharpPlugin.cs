using System;

namespace Pluton
{
    /// <summary>
    /// C# engine.
    /// </summary>
    public class CSharpPlugin
    {
        /// <summary>
        /// The commands.
        /// </summary>
        public ChatCommands Commands;

        /// <summary>
        /// The data store.
        /// </summary>
        public DataStore DataStore;

        /// <summary>
        /// The plugin.
        /// </summary>
        public BasePlugin Plugin;

        /// <summary>
        /// The util.
        /// </summary>
        public Util Util;

        /// <summary>
        /// The server.
        /// </summary>
        public Server Server;

        /// <summary>
        /// The server console commands.
        /// </summary>
        public ConsoleCommands ServerConsoleCommands;

        /// <summary>
        /// The web.
        /// </summary>
        public Web Web;

        /// <summary>
        /// The world.
        /// </summary>
        public World World;

        /// <summary>
        /// Find instance for C# plugins.
        /// </summary>
        /// <value>The find class.</value>
        public Find Find {
            get {
                return Find.Instance;
            }
        }

        /// <summary>
        /// A global storage that any plugin can easily access.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> GlobalData {
            get {
                return BasePlugin.GlobalData;
            }
        }

        public CSharpPlugin()
        {
        }
    }
}

