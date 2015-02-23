using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Pluton.Events;
using System.Net;
using System.Text;

namespace Pluton
{
    public class BasePlugin : CountedInstance, IPlugin
    {
        /// <summary>
        /// Name of the Plugin.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Source code (or path in case of c# plugins).
        /// </summary>
        /// <value>The code.</value>
        public string Code {
            get;
            private set;
        }

        /// <summary>
        /// DirectoryInfo of the directory in which the plugin is in.
        /// </summary>
        /// <value>The root dir.</value>
        public DirectoryInfo RootDir {
            get;
            private set;
        }

        /// <summary>
        /// Global methods of the plugin.
        /// </summary>
        /// <value>The globals.</value>
        public IList<string> Globals {
            get;
            protected set;
        }

        /// <summary>
        /// Dictionary that holds the timers.
        /// </summary>
        public readonly Dictionary<string, TimedEvent> Timers;

        /// <summary>
        /// List of parallel timers.
        /// </summary>
        public readonly List<TimedEvent> ParallelTimers;

        /// <summary>
        /// A global storage that any plugin can easily access.
        /// </summary>
        public static Dictionary<string, object> GlobalData;

        /// <summary>
        /// The console commands.
        /// </summary>
        public ConsoleCommands consoleCommands;

        /// <summary>
        /// The chat commands.
        /// </summary>
        public ChatCommands chatCommands;

        /// <summary>
        /// The type of the plugin.
        /// </summary>
        public PluginType Type = PluginType.Undefined;

        /// <summary>
        /// The current state of the plugin.
        /// </summary>
        public PluginState State = PluginState.NotLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluton.BasePlugin"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="code">Code.</param>
        /// <param name="rootdir">RootDir.</param>
        public BasePlugin(string name, string code, DirectoryInfo rootdir)
        {
            Name = name;
            Code = code;
            RootDir = rootdir;

            Timers = new Dictionary<string, TimedEvent>();
            ParallelTimers = new List<TimedEvent>();
            consoleCommands = new ConsoleCommands(this);
            chatCommands = new ChatCommands(this);
        }

        /// <summary>
        /// Format exceptions to give meaningful reports.
        /// </summary>
        /// <returns>String representation of the exception.</returns>
        /// <param name="ex">The exception object.</param>
        public virtual string FormatException(Exception ex)
        {
            string nuline = Environment.NewLine;
            return ex.Message + nuline + ex.TargetSite.ToString() + nuline + ex.StackTrace;
        }

        /// <summary>
        /// Invoke the specified method and args.
        /// </summary>
        /// <param name="method">Method.</param>
        /// <param name="args">Arguments.</param>
        public virtual object Invoke(string method, params object[] args)
        {
            return null;
        }

        #region file operations

        /// <summary>
        /// Normalizes the path.
        /// </summary>
        /// <returns>The path.</returns>
        /// <param name="path">Path.</param>
        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Validates the relative path.
        /// </summary>
        /// <returns>The relative path.</returns>
        /// <param name="path">Path.</param>
        public string ValidateRelativePath(string path)
        {
            string normalizedPath = NormalizePath(Path.Combine(RootDir.FullName, path));
            string rootDirNormalizedPath = NormalizePath(RootDir.FullName);

            if (!normalizedPath.StartsWith(rootDirNormalizedPath))
                return null;

            return normalizedPath;
        }

        /// <summary>
        /// Creates the dir.
        /// </summary>
        /// <returns><c>true</c>, if dir was created, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
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

        /// <summary>
        /// Deletes the log.
        /// </summary>
        /// <param name="path">Path.</param>
        public void DeleteLog(string path)
        {
            path = ValidateRelativePath(path + ".log");
            if (path == null)
                return;

            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Log the specified text at path.log.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="text">Text.</param>
        public void Log(string path, string text)
        {
            path = ValidateRelativePath(path + ".log");
            if (path == null)
                return;

            File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
        }

        /// <summary>
        /// Rotates the log.
        /// </summary>
        /// <param name="logfile">Logfile.</param>
        /// <param name="max">Max.</param>
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

        #region jsonfiles

        /// <summary>
        /// Wether or not the specified '.json' file exists.
        /// </summary>
        /// <returns><c>true</c>, if the file exists, <c>false</c> otherwise.</returns>
        /// <param name="path">Path to the '.json' file.</param>
        public bool JsonFileExists(string path)
        {
            path = ValidateRelativePath(path + ".json");
            if (path == null)
                return false;

            return File.Exists(path);
        }

        /// <summary>
        /// Reads a '.json' file.
        /// </summary>
        /// <returns>The json string.</returns>
        /// <param name="path">Path to the '.json' file.</param>
        public string FromJsonFile(string path)
        {
            if (JsonFileExists(path))
                return File.ReadAllText(path);

            return null;
        }

        /// <summary>
        /// Saves a json string at the specified path with '.json' extension.
        /// </summary>
        /// <param name="path">File name.</param>
        /// <param name="json">The json string to save.</param>
        public void ToJsonFile(string path, string json)
        {
            path = ValidateRelativePath(path + ".json");
            if (path == null)
                return;

            File.WriteAllText(path, json);
        }

        #endregion

        #region inifiles

        /// <summary>
        /// Gets the ini.
        /// </summary>
        /// <returns>An IniParser object.</returns>
        /// <param name="path">File name.</param>
        public IniParser GetIni(string path)
        {
            path = ValidateRelativePath(path + ".ini");
            if (path == null)
                return (IniParser)null;

            if (File.Exists(path))
                return new IniParser(path);

            return (IniParser)null;
        }

        /// <summary>
        /// Checks if the specified ini file exists.
        /// </summary>
        /// <returns><c>true</c>, if it exists, <c>false</c> otherwise.</returns>
        /// <param name="path">File name.</param>
        public bool IniExists(string path)
        {
            path = ValidateRelativePath(path + ".ini");
            if (path == null)
                return false;

            return File.Exists(path);
        }

        /// <summary>
        /// Creates the ini.
        /// </summary>
        /// <returns>The ini.</returns>
        /// <param name="path">Path.</param>
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

        /// <summary>
        /// Gets the inis.
        /// </summary>
        /// <returns>The inis.</returns>
        /// <param name="path">Path.</param>
        public List<IniParser> GetInis(string path)
        {
            path = ValidateRelativePath(path);
            if (path == null)
                return new List<IniParser>();

            return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
        }

        #endregion

        /// <summary>
        /// Gets the plugin.
        /// </summary>
        /// <returns>The plugin.</returns>
        /// <param name="name">Name.</param>
        public BasePlugin GetPlugin(string name)
        {
            BasePlugin plugin;  
            plugin = PluginLoader.Plugins[name];
            if (plugin == null) {
                Logger.LogDebug("[Plugin][GetPlugin] '" + name + "' plugin not found!");
                return null;
            }
            return plugin;
        }

        #region time

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <returns>The date.</returns>
        public string GetDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        /// <summary>
        /// Gets the ticks.
        /// </summary>
        /// <returns>The ticks.</returns>
        public int GetTicks()
        {
            return Environment.TickCount;
        }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <returns>The time.</returns>
        public string GetTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <returns>The timestamp.</returns>
        public long GetTimestamp()
        {
            TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
            return (long)span.TotalSeconds;
        }

        #endregion

        #region hooks

        public IDisposable OnAllPluginsLoadedHook;
        public void OnAllPluginsLoaded(string s = "") {
            this.Invoke("On_AllPluginsLoaded");
        }

        public IDisposable OnBuildingCompleteHook;
        public void OnBuildingComplete(BuildingPart bp) {
            this.Invoke("On_BuildingComplete", bp);
        }

        public IDisposable OnPlacementHook;
        public void OnPlacement(BuildingEvent be) {
            this.Invoke("On_Placement", be);
        }

        public IDisposable OnChatHook;
        public void OnChat(ChatEvent arg) {
            this.Invoke("On_Chat", arg);
        }

        public IDisposable OnClientAuthHook;
        public void OnClientAuth(AuthEvent ae) {
            this.Invoke("On_ClientAuth", ae);
        }

        public IDisposable OnClientConsoleHook;
        public void OnClientConsole(ClientConsoleEvent ce) {
            ce.ReplyWith(ce.cmd + " " + String.Join(" ", ce.Args.ToArray()) + " was executed!");
            this.Invoke("On_ClientConsole", ce);
        }

        public IDisposable OnCommandHook;
        public void OnCommand(CommandEvent cmd) {
            this.Invoke("On_Command", cmd);
        }

        public IDisposable OnCommandPermissionHook;
        public void OnCommandPermission(CommandPermissionEvent perm) {
            this.Invoke("On_CommandPermission", perm);
        }

        public IDisposable OnCorpseHurtHook;
        public void OnCorpseHurt(CorpseHurtEvent he) {
            this.Invoke("On_CorpseHurt", he);
        }

        public IDisposable OnDoorCodeHook;
        public void OnDoorCode(DoorCodeEvent dc) {
            this.Invoke("On_DoorCode", dc);
        }

        public IDisposable OnDoorUseHook;
        public void OnDoorUse(DoorUseEvent due) {
            this.Invoke("On_DoorUse", due);
        }

        public IDisposable OnLootingEntityHook;
        public void OnLootingEntity(EntityLootEvent le) {
            this.Invoke("On_LootingEntity", le);
        }

        public IDisposable OnLootingItemHook;
        public void OnLootingItem(ItemLootEvent le) {
            this.Invoke("On_LootingItem", le);
        }

        public IDisposable OnLootingPlayerHook;
        public void OnLootingPlayer(PlayerLootEvent le) {
            this.Invoke("On_LootingPlayer", le);
        }

        public IDisposable OnNPCHurtHook;
        public void OnNPCHurt(NPCHurtEvent he) {
            this.Invoke("On_NPCHurt", he);
        }

        public IDisposable OnNPCKilledHook;
        public void OnNPCKilled(NPCDeathEvent de) {
            this.Invoke("On_NPCKilled", de);
        }

        public IDisposable OnPlayerHurtHook;
        public void OnPlayerHurt(PlayerHurtEvent he) {
            this.Invoke("On_PlayerHurt", he);
        }

        public IDisposable OnCombatEntityHurtHook;
        public void OnCombatEntityHurt(CombatEntityHurtEvent he) {
            this.Invoke("On_CombatEntityHurt", he);
        }

        public IDisposable OnPlayerConnectedHook;
        public void OnPlayerConnected(Player player) {
            this.Invoke("On_PlayerConnected", player);
        }

        public IDisposable OnPlayerDiedHook;
        public void OnPlayerDied(PlayerDeathEvent de) {
            this.Invoke("On_PlayerDied", de);
        }

        public IDisposable OnPlayerDisconnectedHook;
        public void OnPlayerDisconnected(Player player) {
            this.Invoke("On_PlayerDisconnected", player);
        }

        public IDisposable OnPlayerGatheringHook;
        public void OnPlayerGathering(GatherEvent ge) {
            this.Invoke("On_PlayerGathering", ge);
        }

        public IDisposable OnPlayerStartCraftingHook;
        public void OnPlayerStartCrafting(CraftEvent ce) {
            this.Invoke("On_PlayerStartCrafting", ce);
        }

        public IDisposable OnPlayerTakeRadiationHook;
        public void OnPlayerTakeRadiation(PlayerTakeRadsEvent re) {
            this.Invoke("On_PlayerTakeRadiation", re);
        }

        public IDisposable OnRespawnHook;
        public void OnRespawn(RespawnEvent re) {
            this.Invoke("On_Respawn", re);
        }

        public IDisposable OnServerConsoleHook;
        public void OnServerConsole(ServerConsoleEvent ce) {
            this.Invoke("On_ServerConsole", ce);
        }

        public IDisposable OnServerInitHook;
        public void OnServerInit(string s = "") {
            this.Invoke("On_ServerInit");
        }

        public IDisposable OnServerShutdownHook;
        public void OnServerShutdown(string s = "") {
            this.Invoke("On_ServerShutdown");
        }

        public void OnTimerCB(TimedEvent evt)
        {
            if (Globals.Contains(evt.Name + "Callback")) {
                Invoke(evt.Name + "Callback", evt);
            }
        }

        #endregion

        #region timer methods

        /// <summary>
        /// Creates a timer.
        /// </summary>
        /// <returns>The timer.</returns>
        /// <param name="name">Name.</param>
        /// <param name="timeoutDelay">Timeout delay.</param>
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

        /// <summary>
        /// Creates a timer.
        /// </summary>
        /// <returns>The timer.</returns>
        /// <param name="name">Name.</param>
        /// <param name="timeoutDelay">Timeout delay.</param>
        /// <param name="args">Arguments.</param>
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

        /// <summary>
        /// Gets a timer.
        /// </summary>
        /// <returns>The timer.</returns>
        /// <param name="name">Name.</param>
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

        /// <summary>
        /// Kills the timer.
        /// </summary>
        /// <param name="name">Name.</param>
        public void KillTimer(string name)
        {
            TimedEvent timer = GetTimer(name);
            if (timer == null)
                return;

            timer.Kill();
            Timers.Remove(name);
        }

        /// <summary>
        /// Kills the timers.
        /// </summary>
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

        /// <summary>
        /// Creates a parallel timer.
        /// </summary>
        /// <returns>The parallel timer.</returns>
        /// <param name="name">Name.</param>
        /// <param name="timeoutDelay">Timeout delay.</param>
        /// <param name="args">Arguments.</param>
        public TimedEvent CreateParallelTimer(string name, int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(name, (double)timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += new TimedEvent.TimedEventFireDelegate(OnTimerCB);
            ParallelTimers.Add(timedEvent);
            return timedEvent;
        }

        /// <summary>
        /// Gets the parallel timer.
        /// </summary>
        /// <returns>The parallel timer.</returns>
        /// <param name="name">Name.</param>
        public List<TimedEvent> GetParallelTimer(string name)
        {
            return (from timer in ParallelTimers
                where timer.Name == name
                select timer).ToList();
        }

        /// <summary>
        /// Kills the parallel timer.
        /// </summary>
        /// <param name="name">Name.</param>
        public void KillParallelTimer(string name)
        {
            foreach (TimedEvent timer in GetParallelTimer(name)) {
                timer.Kill();
                ParallelTimers.Remove(timer);
            }
        }

        #endregion

        #region WEB

        /// <summary>
        /// GET request.
        /// </summary>
        /// <param name="url">URL.</param>
        public string GET(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// POST request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="data">Data.</param>
        public string POST(string url, string data)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                byte[] bytes = client.UploadData(url, "POST", Encoding.ASCII.GetBytes(data));
                return Encoding.ASCII.GetString(bytes);
            }
        }

        /// <summary>
        /// POSTs a json string to the specified url.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="json">Json.</param>
        public string POSTJSON(string url, string json)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                byte[] bytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(json));
                return Encoding.UTF8.GetString(bytes);
            }
        }

        #endregion

        /// <summary>
        /// Creates a Dictionary<string, object> object.</c>.
        /// </summary>
        /// <returns>The dictionary.</returns>
        /// <param name="cap">Capacity.</param>
        public Dictionary<string, object> CreateDict(int cap = 10)
        {
            return new Dictionary<string, object>(cap);
        }
    }
}

