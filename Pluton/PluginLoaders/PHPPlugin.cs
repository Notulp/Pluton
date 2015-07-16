using System;
using System.IO;
using PHP.Core;

namespace Pluton.PluginLoaders
{
    /// <summary>
    /// PHP plugin.
    /// </summary>
    public class PHPPlugin : BasePlugin
    {
        ScriptContext context;
        public PhpArray PHPGlobals;
        PhpObject Class;
        private DirectoryInfo rpath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.PHPPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public PHPPlugin(string name, string code, DirectoryInfo rootdir)
            : base(name, rootdir)
        {
            rpath = rootdir;
            Type = PluginType.PHP;

            if (CoreConfig.GetInstance().GetBoolValue("php", "checkHash") && !code.VerifyMD5Hash())
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
                        var caller = new PhpCallback(Class, func);
                        result = caller.Invoke(args);
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
            try
            {
                context = ScriptContext.CurrentContext;
                context.Include(rpath  + "\\" + Name + ".php", true);
                Class = (PhpObject) context.NewObject(Name);
                PHPGlobals = context.GlobalVariables;
                context.GlobalVariables.Add("Commands", chatCommands);
                context.GlobalVariables.Add("DataStore", DataStore.GetInstance());
                context.GlobalVariables.Add("Find", Find.GetInstance());
                context.GlobalVariables.Add("GlobalData", GlobalData);
                context.GlobalVariables.Add("Plugin", this);
                context.GlobalVariables.Add("Server", Pluton.Server.GetInstance());
                context.GlobalVariables.Add("ServerConsoleCommands", consoleCommands);
                context.GlobalVariables.Add("Util", Util.GetInstance());
                context.GlobalVariables.Add("Web", Web.GetInstance());
                context.GlobalVariables.Add("World", World.GetInstance());
                foreach (var x in PHPGlobals)
                {
                    Globals.Add(x.Key.ToString());
                }

                State = PluginState.Loaded;
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
