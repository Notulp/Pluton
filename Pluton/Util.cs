namespace Pluton {
	using Facepunch.Utility;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Text.RegularExpressions;
	using UnityEngine;

	public class Util {
		private readonly Dictionary<string, System.Type> typeCache = new Dictionary<string, System.Type>();
		private static Util util;
		private static DirectoryInfo UtilPath;

		public void ConsoleLog(string str, bool adminOnly = false) {
			try {
				// FIXME
				/*foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players) {

					if (!adminOnly) {
						ConsoleNetworker.singleton.networkView.RPC<string>("CL_ConsoleMessage", player.PlayerClient.netPlayer, str);
					} else if (player.Admin) {
						ConsoleNetworker.singleton.networkView.RPC<string>("CL_ConsoleMessage", player.PlayerClient.netPlayer, str);
					}
				}*/
			} catch (Exception ex) {
				Logger.LogDebug("ConsoleLog ex");
				Logger.LogException(ex);
			}
		}

		public void DestroyObject(GameObject go) {
			// FIXME
		/*	NetCull.Destroy(go);*/
		}

		// Dumper methods
		public bool DumpObjToFile(string path, object obj, string prefix = "") {
			return DumpObjToFile(path, obj, 1, 30, false, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, string prefix = "") {
			return DumpObjToFile(path, obj, depth, 30, false, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, int maxItems, string prefix = "") {
			return DumpObjToFile(path, obj, depth, maxItems, false, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, int maxItems, bool disPrivate, string prefix = "") {
			return DumpObjToFile(path, obj, depth, maxItems, disPrivate, false, prefix);
		}

		public bool DumpObjToFile(string path, object obj, int depth, int maxItems, bool disPrivate, bool fullClassName, string prefix = "") {
			path = DataStore.GetInstance().RemoveChars(path);
			path = Path.Combine(UtilPath.FullName, path);
			if (path == null)
				return false;

			string result = string.Empty;

			var settings = new DumpSettings();
			settings.MaxDepth = depth;
			settings.MaxItems = maxItems;
			settings.DisplayPrivate = disPrivate;
			settings.UseFullClassNames = fullClassName;
			result = Dump.ToDump(obj, obj.GetType(), prefix, settings);

			string dumpHeader =
				"Object type: " + obj.GetType().ToString() + "\r\n" +
				"TimeNow: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\r\n" +
				"Depth: " + depth.ToString() + "\r\n" +
				"MaxItems: " + maxItems.ToString() + "\r\n" +
				"ShowPrivate: " + disPrivate.ToString() + "\r\n" +
				"UseFullClassName: " + fullClassName.ToString() + "\r\n\r\n";

			File.AppendAllText(path, dumpHeader);
			File.AppendAllText(path, result + "\r\n\r\n");
			return true;
		}
		// dumper end

		public static string NormalizePath(string path) {
			string normal = path.Replace(@"\\", @"\").Replace(@"//", @"/").Trim();
			return normal;
		}

		public static string GetAbsoluteFilePath(string fileName) {
			return Path.Combine(Config.GetPublicFolder(), fileName);
		}

		public static string GetRootFolder() {
			return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
		}

		public static string GetServerFolder() {
			return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))), "RustDedicated_Data");
		}

		public static Util GetUtil() {
			if (util == null) {
				util = new Util();
				UtilPath = new DirectoryInfo (Path.Combine (Config.GetPublicFolder (), "Util"));
			}
			return util;
		}

		public float GetVectorsDistance(Vector3 v1, Vector3 v2) {
			return Vector3.Distance(v1, v2);
		}

		public static Hashtable HashtableFromFile(string path) {
			using (FileStream stream = new FileStream(path, FileMode.Open)) {
				BinaryFormatter formatter = new BinaryFormatter();
				return (Hashtable)formatter.Deserialize(stream);
			}
		}

		public static void HashtableToFile(Hashtable ht, string path) {
			using (FileStream stream = new FileStream(path, FileMode.Create)) {
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, ht);
			}
		}

		// FIXME
		/*
		public Vector3 Infront(Player p, float length) {
			return (p.Location + ((Vector3)(forward * length)));
		}*/

		public object InvokeStatic(string className, string method, object[] args) {
			System.Type type;
			if (!this.TryFindType(className.Replace('.', '+'), out type)) {
				return null;
			}
			MethodInfo info = type.GetMethod(method, BindingFlags.Static);
			if (info == null) {
				return null;
			}
			if (info.ReturnType == typeof(void)) {
				info.Invoke(null, args);
				return true;
			}
			return info.Invoke(null, args);
		}

		public bool IsNull(object obj) {
			return (obj == null);
		}

		public void Log(string str) {
			Logger.Log(str);
		}

		public Match Regex(string input, string match) {
			return new System.Text.RegularExpressions.Regex(input).Match(match);
		}

		public Quaternion RotateX(Quaternion q, float angle) {
			return (q *= Quaternion.Euler(angle, 0f, 0f));
		}

		public Quaternion RotateY(Quaternion q, float angle) {
			return (q *= Quaternion.Euler(0f, angle, 0f));
		}

		public Quaternion RotateZ(Quaternion q, float angle) {
			return (q *= Quaternion.Euler(0f, 0f, angle));
		}

		// FIXUS
		/*
		public static void say(uLink.NetworkPlayer player, string playername, string arg) {
			ConsoleNetworker.SendClientCommand(player, "chat.add " + playername + " " + arg);
		}

		public static void sayAll(string customName, string arg) {
			ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(customName) + " " + Facepunch.Utility.String.QuoteSafe(arg));
		}

		public static void sayAll(string arg) {
			ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
		}

		public static void sayUser(uLink.NetworkPlayer player, string arg) {
			ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
		}

		public static void sayUser(uLink.NetworkPlayer player, string customName, string arg) {
			ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(customName) + " " + Facepunch.Utility.String.QuoteSafe(arg));
		}*/

		public void SetStaticField(string className, string field, object val) {
			System.Type type;
			if (this.TryFindType(className.Replace('.', '+'), out type)) {
				FieldInfo info = type.GetField(field, BindingFlags.Public | BindingFlags.Static);
				if (info != null) {
					info.SetValue(null, Convert.ChangeType(val, info.FieldType));
				}
			}
		}

		public bool TryFindType(string typeName, out System.Type t) {
			lock (this.typeCache) {
				if (!this.typeCache.TryGetValue(typeName, out t)) {
					foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
						t = assembly.GetType(typeName);
						if (t != null) {
							break;
						}
					}
					this.typeCache[typeName] = t;
				}
			}
			return (t != null);
		}

		public System.Type TryFindReturnType(string typeName) {
			System.Type t;
			if (this.TryFindType(typeName, out t))
				return t;
			throw new Exception("Type not found " + typeName);
		}

		// need this?
		public bool ContainsString(string str, string key) {
			if (str.Contains(key))
				return true;
			return false;
		}

		// do we need to fix it?
		/*
		public ItemDataBlock ConvertNameToData(string name) {
			ItemDataBlock byName = DatablockDictionary.GetByName(name);
			if (byName != null)
				return byName;
			return null;
		}*/
	}
}