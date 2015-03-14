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
        public DataStore DataStore {
            get {
                return DataStore.GetInstance();
            }
        }

        /// <summary>
        /// The plugin.
        /// </summary>
        public BasePlugin Plugin;

        /// <summary>
        /// The util.
        /// </summary>
        public Util Util {
            get {
                return Util.GetInstance();
            }
        }

        /// <summary>
        /// The server.
        /// </summary>
        public Server Server {
            get {
                return Server.GetInstance();
            }
        }

        /// <summary>
        /// The server console commands.
        /// </summary>
        public ConsoleCommands ServerConsoleCommands;

        /// <summary>
        /// The web.
        /// </summary>
        public Web Web {
            get {
                return Web.GetInstance();
            }
        }

        /// <summary>
        /// The world.
        /// </summary>
        public World World {
            get {
                return World.GetInstance();
            }
        }

        /// <summary>
        /// Find instance for C# plugins.
        /// </summary>
        /// <value>The find class.</value>
        public Find Find {
            get {
                return Find.GetInstance();
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

