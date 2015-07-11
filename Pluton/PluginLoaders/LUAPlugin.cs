using System;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Pluton
{
    /// <summary>
    /// Lua plugin.
    /// </summary>
    public class LUAPlugin : BasePlugin
    {

        /// <summary>
        /// LUA Tables
        /// </summary>
        public Table Tables;
        public Script script;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.LUAPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public LUAPlugin(string name, string code, DirectoryInfo rootdir)
            : base(name, rootdir)
        {
            Type = PluginType.Lua;

            if (CoreConfig.GetInstance().GetBoolValue("lua", "checkHash") && !code.VerifyMD5Hash())
            {
                Logger.LogDebug(String.Format("[Plugin] MD5Hash not found for: {0} [{1}]!", name, Type));
                State = PluginState.HashNotFound;
                return;
            }

            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(a => Load(code)), null);
        }

        /// <summary>
        /// Invoke the specified method and args.
        /// </summary>
        /// <param name="method">Method.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="func">Func.</param>
        public override object Invoke(string func, params object[] args)
        {
            try
            {
                if (State == PluginState.Loaded && Globals.Contains(func))
                {
                    object result = (object)null;

                    using (new Stopper(Name, func))
                    {
                        result = script.Call(func, args);
                    }
                    return result;
                }
                else
                {
                    Logger.LogWarning("[Plugin] Function: " + func + " not found in plugin: " + Name + ", or plugin is not loaded.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                string fileinfo = (String.Format("{0}<{1}>.{2}()", Name, Type, func) + Environment.NewLine);
                Logger.LogError(fileinfo + FormatException(ex));
                return null;
            }
        }

        public override void Load(string code = "")
        {
            UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;
            script = new Script();
            script.Globals.Set("Util", UserData.Create(Util.GetUtil()));
            script.Globals.Set("Plugin", UserData.Create(this));
            script.Globals.Set("Server", UserData.Create(Server.GetInstance()));
            script.Globals.Set("DataStore", UserData.Create(DataStore.GetInstance()));
            script.Globals.Set("Commands", UserData.Create(chatCommands));
            script.Globals.Set("GlobalData", UserData.Create(GlobalData));
            script.Globals.Set("ServerConsoleCommands", UserData.Create(consoleCommands));
            script.Globals.Set("Web", UserData.Create(Web.GetInstance()));
            script.Globals.Set("World", UserData.Create(World.GetInstance()));
            try
            {
                script.DoString(code);
                State = PluginState.Loaded;
                foreach (DynValue v in script.Globals.Keys)
                {
                    Globals.Add(v.ToString().Replace('"'.ToString(), ""));
                }
                Tables = script.Globals;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                State = PluginState.FailedToLoad;
            }

            PluginLoader.GetInstance().OnPluginLoaded(this);
        }
    }
}

