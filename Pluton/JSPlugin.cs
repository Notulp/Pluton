using System;
using System.IO;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Parser;
using Jint.Parser.Ast;

namespace Pluton
{
    public class JSPlugin : BasePlugin
    {
        public Jint.Engine Engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.JSPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public JSPlugin(string name, string code, DirectoryInfo rootdir) : base(name, code, rootdir)
        {
            Type = PluginType.JavaScript;

            if (CoreConfig.GetBoolValue("javascript", "checkHash") && !code.VerifyMD5Hash()) {
                Logger.LogDebug(String.Format("[Plugin] MD5Hash not found for: {0} [{1}]!", name, Type));
                State = PluginState.HashNotFound;
                return;
            }

            Engine = new Jint.Engine(cfg => cfg.AllowClr(typeof(UnityEngine.GameObject).Assembly))
                .SetValue("Server", Server.GetServer())
                .SetValue("DataStore", DataStore.GetInstance())
                .SetValue("Util", Util.GetUtil())
                .SetValue("World", World.GetWorld())
                .SetValue("Plugin", this)
                .SetValue("Commands", chatCommands)
                .SetValue("ServerConsoleCommands", consoleCommands)
                .SetValue("GlobalData", GlobalData)
                .SetValue("Find", Find.Instance)
                .Execute(code);
            JavaScriptParser parser = new JavaScriptParser();
            Globals = (from function in parser.Parse(code).FunctionDeclarations
                select function.Id.Name).ToList<string>();

            State = PluginState.Loaded;
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
                        result = Engine.Invoke(func, args);
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
    }
}

