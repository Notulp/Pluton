using System;

namespace Pluton
{
    public class CSharpPlugin
    {
        public Server Server;
        public DataStore DataStore;
        public Plugin Plugin;
        public World World;
        public Util Util;
        public ChatCommands Commands;
        public ConsoleCommands ServerConsoleCommands;
        public System.Collections.Generic.Dictionary<string, object> GlobalData {
            get {
                return Plugin.GlobalData;
            }
        }

        public CSharpPlugin()
        {

        }
    }
}

