namespace Pluton {
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Globalization;
	using System.ComponentModel;
	using System.Collections.Generic;
	using System.Collections;
	using System.IO;
	using Pluton.Events;
	using Microsoft.Scripting.Hosting;

	public class Plugin {

		public readonly string Name;
		public readonly string Code;
		public readonly object Class;
		public readonly DirectoryInfo RootDir;
		public readonly ScriptEngine Engine;
		public readonly ScriptScope Scope;
		public readonly IList<string> Globals;

		public readonly Dictionary<string, TimedEvent> Timers;

		public Plugin(string name, string code, DirectoryInfo path) {
			Name = name;
			Code = code;
			RootDir = path;
			Timers = new Dictionary<string, TimedEvent>();

			Engine = IronPython.Hosting.Python.CreateEngine();
			Scope = Engine.CreateScope();
			Scope.SetVariable("Plugin", this);
			Scope.SetVariable("Server", Pluton.Server.GetServer());
			Scope.SetVariable("DataStore", DataStore.GetInstance());
			Scope.SetVariable("Util", Util.GetUtil());
			Engine.Execute(code, Scope);
			Class = Engine.Operations.Invoke(Scope.GetVariable(name));
			Globals = Engine.Operations.GetMemberNames(Class);
		}

		public void Invoke(string func, params object[] obj) {
			try {
				if (Globals.Contains(func))
					Engine.Operations.InvokeMember(Class, func, obj);
				else
					Logger.LogDebug("[Pluton] Function: " + func + " not found in plugin: " + Name);
			} catch (Exception ex) {
				Logger.LogException(ex);
			}
		}

		#region file operations

		private static string NormalizePath(string path) {
			return Path.GetFullPath(new Uri(path).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		private string ValidateRelativePath(string path) {
			string normalizedPath = NormalizePath(Path.Combine(RootDir.FullName, path));
			string rootDirNormalizedPath = NormalizePath(RootDir.FullName);

			if (!normalizedPath.StartsWith(rootDirNormalizedPath))
				return null;

			return normalizedPath;
		}

		public bool CreateDir(string path) {
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

		public void DeleteLog(string path) {
			path = ValidateRelativePath(path + ".log");
			if (path == null)
				return;

			if (File.Exists(path))
				File.Delete(path);
		}

		public void Log(string path, string text) {
			path = ValidateRelativePath(path + ".log");
			if (path == null)
				return;

			File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
		}

		public void RotateLog(string logfile, int max = 6) {
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
					Logger.LogError("[Pluton] RotateLog " + logfile + ", " + pathh + ", " + pathi + ", " + ex.StackTrace);
					continue;
				}
			}
		}

		#endregion

		#region inifiles

		public IniParser GetIni(string path) {
			path = ValidateRelativePath(path + ".ini");
			if (path == null)
				return (IniParser)null;

			if (File.Exists(path))
				return new IniParser(path);

			return (IniParser)null;
		}

		public bool IniExists(string path) {
			path = ValidateRelativePath(path + ".ini");
			if (path == null)
				return false;

			return File.Exists(path);
		}

		public IniParser CreateIni(string path) {
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

		public List<IniParser> GetInis(string path) {
			path = ValidateRelativePath(path);
			if (path == null)
				return new List<IniParser>();

			return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
		}

		#endregion

		public Plugin GetPlugin(string name) {
			Plugin plugin;	
			plugin = PluginLoader.Plugins[name];
			if (plugin == null) {
				Logger.LogDebug("[IPModule] [GetPlugin] '" + name + "' plugin not found!");
				return null;
			}
			return plugin;
		}

		#region time
		// CONSIDER: putting these into a separate class along with some new shortcut
		//				Time.GetDate() looks more appropriate than Plugin.GetDate()
		public string GetDate() {
			return DateTime.Now.ToShortDateString();
		}

		public int GetTicks() {
			return Environment.TickCount;
		}

		public string GetTime() {
			return DateTime.Now.ToShortTimeString();
		}

		public long GetTimestamp() {
			TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
			return (long)span.TotalSeconds;
		}

		#endregion

		#region hooks
		public void OnBlueprintUse() {
			throw new NotImplementedException("There is no OnBlueprintUse hook yet!");
		}

		public void OnChat(ConsoleSystem.Arg arg) {
			this.Invoke("On_Chat", new object[] { arg });
		}

		public void OnCommand(Player player, string command, string[] args) {
			this.Invoke("On_Command", new object[] { player, command, args });
		}

		public void OnConsole(ref ConsoleSystem.Arg arg, bool external) {
			throw new NotImplementedException("There is no OnConsole hook yet!");
		}

		public void OnCorpseDropped(BaseCorpse corpse, Entity parent) {
			this.Invoke("On_CorpseDropped", new object[] { corpse, parent });
		}

		public void OnCorpseAttacked(BaseCorpse corpse, HitInfo info) {
			this.Invoke("On_CorpseAttacked", new object[] { corpse, info });
		}

		public void OnBuildingComplete(BuildingPart bp) {
			this.Invoke("On_BuildingComplete", new object[] { bp });
		}

		public void OnBuildingUpdate(BuildingEvent evt) {
			this.Invoke("On_BuildingUpdate", new object[] { evt });
		}

		public void OnBuildingPartAttacked(BuildingPart bp, HitInfo info) {
			this.Invoke("On_BuildingPartAttacked", new object[] { bp, info });
		}

		public void OnBuildingPartDestroyed(BuildingPart bp, HitInfo info) {
			this.Invoke("On_BuildingPartDestroyed", new object[] { bp, info });
		}

		public void OnDoorUse() {
			throw new NotImplementedException("There is no OnDoorUse hook yet!");
		}

		public void OnEntityDecay() {
			throw new NotImplementedException("There is no OnEntityDecay hook yet!");
		}

		public void OnFrameDeployed(BuildingPart bp) {
			this.Invoke("On_FrameDeployed", new object[] { bp });
		}

		public void OnNPCHurt(NPCHurtEvent evt) {
			this.Invoke("On_NPCAttacked", new object[] { evt });
		}

		public void OnNPCKilled(NPCDeathEvent evt) {
			this.Invoke("On_NPCKilled", new object[] { evt });
		}

		public void OnLootingEntity(EntityLootEvent evt) {
			this.Invoke("On_LootingEntity", new object[] { evt });
		}

		public void OnLootingPlayer(PlayerLootEvent evt) {
			this.Invoke("On_LootingPlayer", new object[] { evt });
		}

		public void OnLootingItem(ItemLootEvent evt) {
			this.Invoke("On_LootingItem", new object[] { evt });
		}

		public void OnPlayerConnected(Player player) {
			this.Invoke("On_PlayerConnected", new object []{ player });
		}

		public void OnPlayerDisconnected(Player player) {
			this.Invoke("On_PlayerDisconnected", new object[] { player });
		}

		public void OnPlayerGathering(GatherEvent evt) {
			this.Invoke("On_PlayerGathering", new object[] { evt });
		}

		public void OnPlayerAttacked(PlayerHurtEvent evt) {
			this.Invoke("On_PlayerAttacked", new object[] { evt });
		}

		public void OnPlayerDied(PlayerDeathEvent evt) {
			this.Invoke("On_PlayerDied", new object[] { evt });
		}

		public void OnPlayerTakeDamage(Player player, float dmgAmount, Rust.DamageType dmgType) {
			this.Invoke("On_PlayerTakeDamage", new object[] { player, dmgAmount, dmgType });
		}

		public void OnPlayerTakeRadiation(Player player, float radAmount) {
			this.Invoke("On_PlayerTakeRadiation", new object[] { player, radAmount });
		}

		public void OnServerInit() {
			throw new NotImplementedException("There is no OnServerInit hook yet!");
		}

		public void OnServerShutdown() {
			throw new NotImplementedException("There is no OnServerShutdown hook yet!");
		}

		// timer hooks

		public void OnTimerCB(string name) {
			if (Globals.Contains(name + "Callback")) {
				Invoke(name + "Callback", new object[0]);
			}
		}

		public void OnTimerCBArgs(string name, Dictionary<string, object> args) {
			if (Globals.Contains(name + "Callback")) {
				Invoke(name + "Callback", args);
			}
		}

		#endregion

		#region timer methods

		public TimedEvent CreateTimer(string name, int timeoutDelay) {
			TimedEvent timedEvent = GetTimer(name);
			if (timedEvent == null) {
				timedEvent = new TimedEvent(name, (double)timeoutDelay);
				timedEvent.OnFire += new TimedEvent.TimedEventFireDelegate(OnTimerCB);
				Timers.Add(name, timedEvent);
			}
			return timedEvent;
		}

		public TimedEvent CreateTimer(string name, int timeoutDelay, Dictionary<string, object> args) {
			TimedEvent timedEvent = GetTimer(name);
			if (timedEvent == null) {
				timedEvent = new TimedEvent(name, (double)timeoutDelay);
				timedEvent.Args = args;
				timedEvent.OnFireArgs += new TimedEvent.TimedEventFireArgsDelegate(OnTimerCBArgs);
				Timers.Add(name, timedEvent);
			}
			return timedEvent;
		}

		public TimedEvent GetTimer(string name) {
			TimedEvent result;
			if (Timers.ContainsKey(name)) {
				result = Timers[name];
			} else {
				result = null;
			}
			return result;
		}

		public void KillTimer(string name) {
			TimedEvent timer = GetTimer(name);
			if (timer != null)
				return;

			timer.Stop();
			Timers.Remove(name);
		}

		public void KillTimers() {
			foreach (TimedEvent current in Timers.Values) {
				current.Stop();
			}
			Timers.Clear();
		}

		#endregion

		// temporarly, for my laziness
		public Dictionary<string, object> CreateDict() {
			return new Dictionary<string, object>();
		}
	}
}

