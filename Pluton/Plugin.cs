namespace Pluton
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Globalization;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Collections;
    using System.IO;
    using Pluton.Events;
    using Jint;
    using Jint.Native;
    using Jint.Parser;
    using Jint.Parser.Ast;
    using Microsoft.Scripting.Hosting;

    public class Plugin
    {
        public readonly string Name;
        public readonly string Code;
        public readonly object Class;
        public readonly PluginType Type;
        public readonly DirectoryInfo RootDir;
        public readonly ScriptEngine PyEngine;
        public readonly Engine JSEngine;
        public readonly ScriptScope Scope;
        public readonly IList<string> Globals;
        public readonly Dictionary<string, TimedEvent> Timers;
        public readonly List<TimedEvent> ParallelTimers;
        public static string LibPath;

        public enum PluginType { Python, JS }

        public Plugin(string name, string code, DirectoryInfo path, PluginType type)
        {
            Name = name;
            Code = code;
            RootDir = path;
            Type = type;
            Timers = new Dictionary<string, TimedEvent>();
            ParallelTimers = new List<TimedEvent>();

            if (type == PluginType.Python) {
                PyEngine = IronPython.Hosting.Python.CreateEngine();
                Scope = PyEngine.CreateScope();
                Scope.SetVariable("Plugin", this);
                Scope.SetVariable("Server", Pluton.Server.GetServer());
                Scope.SetVariable("DataStore", DataStore.GetInstance());
                Scope.SetVariable("Util", Util.GetUtil());
                Scope.SetVariable("World", World.GetWorld());
                Scope.SetVariable("Web", new Web());
                Scope.SetVariable("Commands", PluginCommands.GetInstance());
                PyEngine.Execute(code, Scope);
                Class = PyEngine.Operations.Invoke(Scope.GetVariable(name));
                Globals = PyEngine.Operations.GetMemberNames(Class);
            } else {
                JSEngine = new Engine(cfg => cfg.AllowClr(typeof(UnityEngine.GameObject).Assembly,
                    typeof(PlayerInventory).Assembly))
                    .SetValue("Server", Server.GetServer())
                    .SetValue("DataStore", DataStore.GetInstance())
                    .SetValue("Util", Util.GetUtil())
                    .SetValue("World", World.GetWorld())
                    .SetValue("Web", new Web())
                    .SetValue("Plugin", this)
                    .SetValue("Commands", PluginCommands.GetInstance())
                    .Execute(code);
                JavaScriptParser parser = new JavaScriptParser();
                Globals = (from function in parser.Parse(Code).FunctionDeclarations
                           where (function.Id.Name.StartsWith("On_") || function.Id.Name.EndsWith("Callback"))
                           select function.Id.Name).ToList<string>();
            }
        }

        public void Invoke(string func, params object[] obj)
        {
            try {
                if (Globals.Contains(func)) {
                    if (Type == PluginType.Python)
                        PyEngine.Operations.InvokeMember(Class, func, obj);
                    else
                        JSEngine.Invoke(func, obj);
                } else {
                    Logger.LogDebug("[Plugin] Function: " + func + " not found in plugin: " + Name);
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
        }

        #region file operations

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private string ValidateRelativePath(string path)
        {
            string normalizedPath = NormalizePath(Path.Combine(RootDir.FullName, path));
            string rootDirNormalizedPath = NormalizePath(RootDir.FullName);

            if (!normalizedPath.StartsWith(rootDirNormalizedPath))
                return null;

            return normalizedPath;
        }

        public bool CreateDir(string path)
        {
            try {
                path = ValidateRelativePath(path);
                if (path == null)
                    return false;

                if (Directory.Exists(path))
                    return true;

                Directory.CreateDirectory(path);
                return true;
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
            return false;
        }

        public void DeleteLog(string path)
        {
            path = ValidateRelativePath(path + ".log");
            if (path == null)
                return;

            if (File.Exists(path))
                File.Delete(path);
        }

        public void Log(string path, string text)
        {
            path = ValidateRelativePath(path + ".log");
            if (path == null)
                return;

            File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
        }

        public void RotateLog(string logfile, int max = 6)
        {
            logfile = ValidateRelativePath(logfile + ".log");
            if (logfile == null)
                return;

            string pathh, pathi;
            int i, h;
            for (i = max, h = i - 1; i > 1; i--, h--) {
                pathi = ValidateRelativePath(logfile + i + ".log");
                pathh = ValidateRelativePath(logfile + h + ".log");

                try {
                    if (!File.Exists(pathi))
                        File.Create(pathi);

                    if (!File.Exists(pathh)) {
                        File.Replace(logfile, pathi, null);
                    } else {
                        File.Replace(pathh, pathi, null);
                    }
                } catch (Exception ex) {
                    Logger.LogError("[Plugin] RotateLog " + logfile + ", " + pathh + ", " + pathi + ", " + ex.StackTrace);
                    continue;
                }
            }
        }

        #endregion

        #region inifiles

        public IniParser GetIni(string path)
        {
            path = ValidateRelativePath(path + ".ini");
            if (path == null)
                return (IniParser)null;

            if (File.Exists(path))
                return new IniParser(path);

            return (IniParser)null;
        }

        public bool IniExists(string path)
        {
            path = ValidateRelativePath(path + ".ini");
            if (path == null)
                return false;

            return File.Exists(path);
        }

        public IniParser CreateIni(string path)
        {
            try {
                path = ValidateRelativePath(path + ".ini");
                if (path == null)
                    return (IniParser)null;

                File.WriteAllText(path, "");
                return new IniParser(path);
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
            return (IniParser)null;
        }

        public List<IniParser> GetInis(string path)
        {
            path = ValidateRelativePath(path);
            if (path == null)
                return new List<IniParser>();

            return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
        }

        #endregion

        public Plugin GetPlugin(string name)
        {
            Plugin plugin;	
            plugin = PluginLoader.Plugins[name];
            if (plugin == null) {
                Logger.LogDebug("[Plugin] [GetPlugin] '" + name + "' plugin not found!");
                return null;
            }
            return plugin;
        }

        #region time

        // CONSIDER: putting these into a separate class along with some new shortcut
        //				Time.GetDate() looks more appropriate than Plugin.GetDate()
        public string GetDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        public int GetTicks()
        {
            return Environment.TickCount;
        }

        public string GetTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        public long GetTimestamp()
        {
            TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
            return (long)span.TotalSeconds;
        }

        #endregion

        #region hooks

        public void OnBlueprintUse() {
            throw new NotImplementedException("There is no OnBlueprintUse hook yet!");
        }

        public IDisposable OnChatHook;
        public void OnChat(Chat arg) {
            this.Invoke("On_Chat", arg);
        }

        public IDisposable OnClientAuthHook;
        public void OnClientAuth(AuthEvent ae) {
            this.Invoke("On_ClientAuth", ae);
        }

        public IDisposable OnCommandHook;
        public void OnCommand(Command cmd) {
            this.Invoke("On_Command", cmd);
        }

        public void OnConsole(ref ConsoleSystem.Arg arg, bool external) {
            throw new NotImplementedException("There is no OnConsole hook yet!");
        }

        public IDisposable OnCorpseDroppedHook;
        public void OnCorpseDropped(CorpseInitEvent ie) {
            this.Invoke("On_CorpseDropped", ie);
        }

        public IDisposable OnCorpseAttackedHook;
        public void OnCorpseAttacked(CorpseHurtEvent he) {
            this.Invoke("On_CorpseAttacked", he);
        }

        public IDisposable OnBuildingCompleteHook;
        public void OnBuildingComplete(BuildingPart bp) {
            this.Invoke("On_BuildingComplete", bp);
        }

        public IDisposable OnBuildingUpdateHook;
        public void OnBuildingUpdate(BuildingEvent be) {
            this.Invoke("On_BuildingUpdate", be);
        }

        public IDisposable OnBuildingPartAttackedHook;
        public void OnBuildingPartAttacked(BuildingHurtEvent he) {
            this.Invoke("On_BuildingPartAttacked", he);
        }

        public IDisposable OnBuildingPartDestroyedHook;
        public void OnBuildingPartDestroyed(BuildingHurtEvent he) {
            this.Invoke("On_BuildingPartDestroyed", he);
        }

        public void OnDoorUse() {
            throw new NotImplementedException("There is no OnDoorUse hook yet!");
        }

        public void OnEntityDecay() {
            throw new NotImplementedException("There is no OnEntityDecay hook yet!");
        }

        public IDisposable OnFrameDeployedHook;
        public void OnFrameDeployed(BuildingPart bp) {
            this.Invoke("On_FrameDeployed", bp);
        }

        public IDisposable OnNPCHurtHook;
        public void OnNPCHurt(NPCHurtEvent he) {
            this.Invoke("On_NPCAttacked", he);
        }

        public IDisposable OnNPCKilledHook;
        public void OnNPCKilled(NPCDeathEvent de) {
            this.Invoke("On_NPCKilled", de);
        }

        public IDisposable OnLootingEntityHook;
        public void OnLootingEntity(EntityLootEvent le) {
            this.Invoke("On_LootingEntity", le);
        }

        public IDisposable OnLootingPlayerHook;
        public void OnLootingPlayer(PlayerLootEvent le) {
            this.Invoke("On_LootingPlayer", le);
        }

        public IDisposable OnLootingItemHook;
        public void OnLootingItem(ItemLootEvent le) {
            this.Invoke("On_LootingItem", le);
        }

        public IDisposable OnPlayerConnectedHook;
        public void OnPlayerConnected(Player player) {
            this.Invoke("On_PlayerConnected", player);
        }

        public IDisposable OnPlayerDisconnectedHook;
        public void OnPlayerDisconnected(Player player) {
            this.Invoke("On_PlayerDisconnected", player);
        }

        public IDisposable OnPlayerGatheringHook;
        public void OnPlayerGathering(GatherEvent ge) {
            this.Invoke("On_PlayerGathering", ge);
        }

        public IDisposable OnPlayerAttackedHook;
        public void OnPlayerAttacked(PlayerHurtEvent he) {
            this.Invoke("On_PlayerAttacked", he);
        }

        public IDisposable OnPlayerDiedHook;
        public void OnPlayerDied(PlayerDeathEvent de) {
            this.Invoke("On_PlayerDied", de);
        }

        public IDisposable OnPlayerTakeDamageHook;
        public void OnPlayerTakeDamage(PlayerTakedmgEvent de) {
            this.Invoke("On_PlayerTakeDamage", de);
        }

        public IDisposable OnPlayerTakeRadiationHook;
        public void OnPlayerTakeRadiation(PlayerTakeRadsEvent re) {
            this.Invoke("On_PlayerTakeRadiation", re);
        }

        public IDisposable OnServerInitHook;
        public void OnServerInit() {
            throw new NotImplementedException("There is no OnServerInit hook yet!");
        }

        public IDisposable OnServerShutdownHook;
        public void OnServerShutdown(string s = "") {
            this.Invoke("On_ServerShutdown");
        }

        public IDisposable OnRespawnHook;
        public void OnRespawn(RespawnEvent re) {
            this.Invoke("On_Respawn", re);
        }

        public IDisposable OnLoadCommandsHook;
        public void OnLoadCommands(string s) {
            this.Invoke("On_LoadingCommands");
        }

        public void OnTimerCB(TimedEvent evt)
        {
            if (Globals.Contains(evt.Name + "Callback")) {
                Invoke(evt.Name + "Callback", evt);
            }
        }

        #endregion

        #region timer methods

        public TimedEvent CreateTimer(string name, int timeoutDelay)
        {
            TimedEvent timedEvent = GetTimer(name);
            if (timedEvent == null) {
                timedEvent = new TimedEvent(name, (double)timeoutDelay);
                timedEvent.OnFire += new TimedEvent.TimedEventFireDelegate(OnTimerCB);
                Timers.Add(name, timedEvent);
            }
            return timedEvent;
        }

        public TimedEvent CreateTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = GetTimer(name);
            if (timedEvent == null) {
                timedEvent = new TimedEvent(name, (double)timeoutDelay);
                timedEvent.Args = args;
                timedEvent.OnFire += new TimedEvent.TimedEventFireDelegate(OnTimerCB);
                Timers.Add(name, timedEvent);
            }
            return timedEvent;
        }

        public TimedEvent GetTimer(string name)
        {
            TimedEvent result;
            if (Timers.ContainsKey(name)) {
                result = Timers[name];
            } else {
                result = null;
            }
            return result;
        }

        public void KillTimer(string name)
        {
            TimedEvent timer = GetTimer(name);
            if (timer != null)
                return;

            timer.Kill();
            Timers.Remove(name);
        }

        public void KillTimers()
        {
            foreach (TimedEvent current in Timers.Values) {
                current.Kill();
            }
            foreach (TimedEvent timer in ParallelTimers) {
                timer.Kill();
            }
            Timers.Clear();
            ParallelTimers.Clear();
        }

        #endregion

        #region ParalellTimers

        public TimedEvent CreateParallelTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(name, (double)timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += new TimedEvent.TimedEventFireDelegate(OnTimerCB);
            ParallelTimers.Add(timedEvent);
            return timedEvent;
        }

        public List<TimedEvent> GetParallelTimer(string name)
        {
            return (from timer in ParallelTimers
                    where timer.Name == name
                    select timer).ToList();
        }

        public void KillParallelTimer(string name)
        {
            foreach (TimedEvent timer in GetParallelTimer(name)) {
                timer.Kill();
                ParallelTimers.Remove(timer);
            }
        }

        #endregion

        public Dictionary<string, object> CreateDict(int cap = 10)
        {
            return new Dictionary<string, object>(cap);
        }
    }
}

