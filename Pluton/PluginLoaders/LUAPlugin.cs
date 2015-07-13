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
        public override object Invoke(string method, params object[] args)
        {
            try {
                if (State == PluginState.Loaded && Globals.Contains(method)) {
                    object result;

                    using (new Stopper(Name, method)) {
                        result = script.Call(script.Globals[method], args);
                    }
                    return result;
                }
                Logger.LogWarning("[Plugin] Function: " + method + " not found in plugin: " + Name + ", or plugin is not loaded.");
                return null;
            }
            catch (Exception ex)
            {
                string fileinfo = (String.Format("{0}<{1}>.{2}()", Name, Type, method) + Environment.NewLine);
                Logger.LogError(fileinfo + FormatException(ex));
                return null;
            }
        }

        public override string FormatException(Exception ex)
        {
            return base.FormatException(ex) +
                (ex is ScriptRuntimeException ? Environment.NewLine + (ex as ScriptRuntimeException).DecoratedMessage : "");
        }

        public override void Load(string code = "")
        {
            try
            {
                UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;
                script = new Script();
                script.Globals["Plugin"] = this;
                script.Globals["Util"] = Util.GetInstance();
                script.Globals["Server"] = Server.GetInstance();
                script.Globals["DataStore"] = DataStore.GetInstance();
                script.Globals["Commands"] = chatCommands;
                script.Globals["GlobalData"] = GlobalData;
                script.Globals["ServerConsoleCommands"] = consoleCommands;
                script.Globals["Web"] = Web.GetInstance();
                script.Globals["World"] = World.GetInstance();
                script.DoString(code);

                Author = script.Globals.Get("Author").String;
                About = script.Globals.Get("About").String;
                Version = script.Globals.Get("Version").String;

                State = PluginState.Loaded;
                foreach (DynValue v in script.Globals.Keys)
                {
                    Globals.Add(v.ToString().Replace("\"", ""));
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

