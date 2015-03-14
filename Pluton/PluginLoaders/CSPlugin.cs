using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;
using System.Linq;

namespace Pluton
{
    /// <summary>
    /// C# plugin.
    /// </summary>
    public class CSPlugin : BasePlugin
    {
        public CSharpPlugin Engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.CSPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public CSPlugin(string name, string code, DirectoryInfo rootdir) : base(name, rootdir)
        {
            Type = PluginType.CSharp;

            //For C# plugins code is the dll path
            byte[] bin = File.ReadAllBytes(code);
            if (CoreConfig.GetInstance().GetBoolValue("csharp", "checkHash") && !bin.VerifyMD5Hash()) {
                Logger.LogDebug(String.Format("[Plugin] MD5Hash not found for: {0} [{1}]!", name, Type));
                State = PluginState.HashNotFound;
                return;
            }

            Assembly assembly = Assembly.Load(bin);
            Type classType = assembly.GetType(name + "." + name);
            if (classType == null || !classType.IsSubclassOf(typeof(CSharpPlugin)) || !classType.IsPublic || classType.IsAbstract)
                throw new TypeLoadException("Main module class not found:" + Name);
            Engine = (CSharpPlugin)Activator.CreateInstance(classType);

            Engine.Plugin = this;
            Engine.Commands = chatCommands;
            Engine.ServerConsoleCommands = consoleCommands;

            Globals = (from method in classType.GetMethods()
                select method.Name).ToList<string>();

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
                        result = Engine.CallMethod(func, args);
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

