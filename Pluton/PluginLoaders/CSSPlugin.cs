using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pluton
{
    /// <summary>
    /// C# script plugin.
    /// </summary>
    public class CSSPlugin : BasePlugin
    {
        public CSharpPlugin Engine;

        public static string compileParams = "/target:library /debug- /optimize+ /out:%PLUGINPATH%%PLUGINNAME%.plugin %REFERENCES% %PLUGINPATH%*.cs";

        string CompilePluginParams = "";
        string CompilationResults = "";
        System.Threading.Mutex mutex = new System.Threading.Mutex();
        public bool Compiled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.CSSPlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">Rootdir.</param>
        public CSSPlugin(string name, DirectoryInfo rootdir) : base(name, rootdir)
        {
            Type = PluginType.CSScript;

            CompilePluginParams = compileParams.Replace("%PLUGINPATH%", rootdir.FullName + Path.DirectorySeparatorChar).Replace("%PLUGINNAME%", name).Replace("%REFERENCES%", String.Join(" ", GetDllPaths().ToArray()));

            System.Threading.ThreadPool.QueueUserWorkItem(
                new System.Threading.WaitCallback(a => Load()), null);
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

        public override void Load(string code = "")
        {
            try {
                LoadReferences();

                Assembly plugin = Compile();

                /*//For C# plugins code is the dll path
                byte[] bin = File.ReadAllBytes(code);
                if (CoreConfig.GetInstance().GetBoolValue("csharp", "checkHash") && !bin.VerifyMD5Hash()) {
                    Logger.LogDebug(String.Format("[Plugin] MD5Hash not found for: {0} [{1}]!", name, Type));
                    State = PluginState.HashNotFound;
                    return;
                }*/
                if (plugin == null) {
                    State = PluginState.FailedToLoad;
                    return;
                }

                Type classType = plugin.GetType(Name + "." + Name);

                if (classType == null || !classType.IsSubclassOf(typeof(CSharpPlugin)) || !classType.IsPublic || classType.IsAbstract) {
                    throw new TypeLoadException("Main module class not found: " + Name);
                }

                Engine = (CSharpPlugin)Activator.CreateInstance(classType);

                Engine.Plugin = this;
                Engine.Commands = chatCommands;
                Engine.ServerConsoleCommands = consoleCommands;

                Globals = (from method in classType.GetMethods()
                                       select method.Name).ToList<string>();

                State = PluginState.Loaded;
            } catch (Exception ex) {
                Logger.LogException(ex);
                State = PluginState.FailedToLoad;
            }

            PluginLoader.GetInstance().OnPluginLoaded(this);
        }

        public Assembly Compile()
        {
            try {
                string mcspath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mcs.exe");

                using (new Stopper("CSSPlugin", "Compile()")) {

                    Process compiler = new Process();

                    compiler.StartInfo.FileName = mcspath;
                    compiler.StartInfo.Arguments = CompilePluginParams;

                    compiler.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    compiler.EnableRaisingEvents = true;

                    compiler.ErrorDataReceived += MCSReturnedErrorData;
                    compiler.Exited += MCSExited;
                    compiler.OutputDataReceived += MCSReturnedOutputData;

                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.StartInfo.RedirectStandardError = true;

                    string temppath = Path.Combine(RootDir.FullName, Name + ".plugin");

                    if (File.Exists(temppath))
                        File.Delete(temppath);

                    compiler.Start();

                    DateTime start = compiler.StartTime;

                    compiler.BeginOutputReadLine();
                    compiler.BeginErrorReadLine();

                    compiler.WaitForExit();

                    Logger.Log("Compile time: " + (compiler.ExitTime - start).ToString());

                    compiler.Close();

                    while (!Compiled) {
                        System.Threading.Thread.Sleep(50);
                    }
                }
                string path = Path.Combine(RootDir.FullName, Name + ".plugin");
                File.WriteAllText(Path.Combine(RootDir.FullName, Name + "_result.txt"), CompilationResults);
                if (File.Exists(path)) {
                    return Assembly.Load(File.ReadAllBytes(path));
                } else {
                    Logger.LogError("returning null for the assembly");
                    return null;
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
                return null;
            }
        }

        void MCSReturnedOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) {
                mutex.WaitOne();
                CompilationResults += e.Data + Environment.NewLine;
                mutex.ReleaseMutex();
            }
        }

        void MCSExited(object sender, EventArgs e)
        {
            Compiled = true;
        }

        void MCSReturnedErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) {
                mutex.WaitOne();
                CompilationResults += e.Data + Environment.NewLine;
                mutex.ReleaseMutex();
            }
        }

        IEnumerable<string> GetDllPaths()
        {
            string refpath = Path.Combine(RootDir.FullName, "References");
            if (Directory.Exists(refpath)) {
                DirectoryInfo refdir = new DirectoryInfo(refpath);
                var files = refdir.GetFiles("*.dll");
                foreach (var file in files) {
                    yield return "/r:" + file.FullName.QuoteSafe();
                }
            }
            string assLoc = Assembly.GetExecutingAssembly().Location.Replace("Pluton.dll", "");
            if (Directory.Exists(assLoc)) {
                var files2 = new DirectoryInfo(assLoc).GetFiles("*.dll");
                foreach (var file2 in files2) {
                    yield return "/r:" + file2.FullName.QuoteSafe();
                }
            }
        }

        public void LoadReferences()
        {
            List<string> dllpaths = GetRefDllPaths().ToList();
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies()) {
                if (dllpaths.Contains(ass.FullName)) {
                    dllpaths.Remove(ass.FullName);
                }
            }
            dllpaths.ForEach(path => {
                Assembly.LoadFile(path);
            });
        }

        IEnumerable<string> GetRefDllPaths()
        {
            string refpath = Path.Combine(RootDir.FullName, "References");
            if (Directory.Exists(refpath)) {
                DirectoryInfo refdir = new DirectoryInfo(refpath);
                FileInfo[] files = refdir.GetFiles("*.dll");
                foreach (FileInfo file in files) {
                    yield return file.FullName;
                }
            }
        }
    }
}

