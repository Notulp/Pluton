using System;
using System.IO;
using System.Linq;
using Microsoft.Scripting.Hosting;

namespace Pluton
{
    /// <summary>
    /// PY plugin.
    /// </summary>
    public class PYPlugin : BasePlugin
    {
        /// <summary>
        /// LibraryPath for python plugins.
        /// </summary>
        public static string LibPath = Path.Combine(Util.GetPublicFolder(), Path.Combine("Python", "Lib"));

        ScriptEngine Engine;
        public ScriptScope Scope;
        object Class;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.PYPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public PYPlugin(string name, string code, DirectoryInfo rootdir) : base(name, rootdir)
        {
            Type = PluginType.Python;

            if (CoreConfig.GetInstance().GetBoolValue("python", "checkHash") && !code.VerifyMD5Hash()) {
                Logger.LogDebug(String.Format("[Plugin] MD5Hash not found for: {0} [{1}]!", name, Type));
                State = PluginState.HashNotFound;
                return;
            }

            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(a => Load(code)), null);
        }

        /// <summary>
        /// Format exceptions to give meaningful reports.
        /// </summary>
        /// <returns>String representation of the exception.</returns>
        /// <param name="ex">The exception object.</param>
        public override string FormatException(Exception ex)
        {
            return base.FormatException(ex) + Environment.NewLine + Engine.GetService<ExceptionOperations>().FormatException(ex);
        }

        /// <summary>
        /// Invoke the specified method and args.
        /// </summary>
        /// <param name="method">Method.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="func">Func.</param>
        public override object Invoke(string func, params object[] args)
        {
            try {
                if (State == PluginState.Loaded && Globals.Contains(func)) {
                    object result = (object)null;

                    using (new Stopper(Name, func)) {
                        result = Engine.Operations.InvokeMember(Class, func, args);
                    }
                    return result;
                } else {
                    Logger.LogWarning("[Plugin] Function: " + func + " not found in plugin: " + Name + ", or plugin is not loaded.");
                    return null;
                }
            } catch (Exception ex) {
                string fileinfo = (String.Format("{0}<{1}>.{2}()", Name, Type, func) + Environment.NewLine);
                Logger.LogError(fileinfo + FormatException(ex));
                return null;
            }
        }

        public override void Load(string code = "")
        {
            Engine = IronPython.Hosting.Python.CreateEngine();
            Scope = Engine.CreateScope();
            Scope.SetVariable("Commands", chatCommands);
            Scope.SetVariable("DataStore", DataStore.GetInstance());
            Scope.SetVariable("Find", Find.GetInstance());
            Scope.SetVariable("GlobalData", GlobalData);
            Scope.SetVariable("Plugin", this);
            Scope.SetVariable("Server", Pluton.Server.GetInstance());
            Scope.SetVariable("ServerConsoleCommands", consoleCommands);
            Scope.SetVariable("Util", Util.GetInstance());
            Scope.SetVariable("Web", Web.GetInstance());
            Scope.SetVariable("World", World.GetInstance());
            try {
                Engine.Execute(code, Scope);
                Class = Engine.Operations.Invoke(Scope.GetVariable(Name));
                Globals = Engine.Operations.GetMemberNames(Class);

                object author = Scope.GetVariable<string>("__author__");
                object about = Scope.GetVariable<string>("__about__");
                object version = Scope.GetVariable<string>("__version__");
                Author = author == null ? "" : author.ToString();
                About = about == null ? "" : about.ToString();
                Version = version == null ? "" : version.ToString();

                State = PluginState.Loaded;
            } catch (Exception ex) {
                Logger.LogException(ex);
                State = PluginState.FailedToLoad;
            }

            PluginLoader.GetInstance().OnPluginLoaded(this);
        }

        public object GetGlobalObject(string identifier)
        {
            try {
                object obj = Scope.GetVariable(identifier);
                return obj;
            } catch {
                return null;
            }
        }
    }
}

