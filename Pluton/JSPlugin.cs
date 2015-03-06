using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Jint;
using Jint.Expressions;

namespace Pluton
{
    public class JSPlugin : BasePlugin
    {
        public JintEngine Engine;
        public Program Program;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.JSPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public JSPlugin(string name, string code, DirectoryInfo rootdir) : base(name, code, rootdir)
        {
            Type = PluginType.JavaScript;

            if (CoreConfig.GetInstance().GetBoolValue("javascript", "checkHash") && !code.VerifyMD5Hash()) {
                Logger.LogDebug(String.Format("[Plugin] MD5Hash not found for: {0} [{1}]!", name, Type));
                State = PluginState.HashNotFound;
                return;
            }

            Engine = new JintEngine(Options.Ecmascript5)
                .AllowClr(true);

            Engine.SetParameter("Commands", chatCommands)
                .SetParameter("DataStore", DataStore.GetInstance())
                .SetParameter("Find", Find.GetInstance())
                .SetParameter("GlobalData", GlobalData)
                .SetParameter("Plugin", this)
                .SetParameter("Server", Server.GetInstance())
                .SetParameter("ServerConsoleCommands", consoleCommands)
                .SetParameter("Util", Util.GetInstance())
                .SetParameter("Web", Web)
                .SetParameter("World", World.GetInstance())
                .SetFunction("importClass", new importit(importClass));

            Program = JintEngine.Compile(code, false);

            Globals = (from statement in Program.Statements
                where statement.GetType() == typeof(FunctionDeclarationStatement)
                select ((FunctionDeclarationStatement)statement).Name).ToList<string>();

            Engine.Run(Program);
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
                        result = Engine.CallFunction(func, args);
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

        public delegate Jint.Native.JsInstance importit(string t);

        public Jint.Native.JsInstance importClass(string type)
        {
            Engine.SetParameter(type.Split('.').Last(), Util.GetInstance().TryFindReturnType(type));
            return (Engine.Global as Jint.Native.JsDictionaryObject)[type.Split('.').Last()];
        }
    }
}

