namespace Pluton
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class Util : Singleton<Util>, ISingleton
    {
        private readonly Dictionary<string, System.Type> typeCache = new Dictionary<string, System.Type>();
        private static DirectoryInfo UtilPath;

        public Dictionary<string, Zone2D> zones = new Dictionary<string, Zone2D>();
        public DataStore ZoneStore;

        public Zone2D GetZone(string name)
        {
            if (zones.ContainsKey(name))
                return zones[name];
            return null;
        }

        public void SetZone(Zone2D zone)
        {
            if (zone == null)
                throw new NullReferenceException("SetZone( zone )");
            zones[zone.Name] = zone;
        }

        public Zone2D CreateZone(string name)
        {
            try {
                GameObject obj = new GameObject(name);
                var gobj = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
                var zone = gobj.AddComponent<Zone2D>();
                zone.Name = name;
                zones[name] = zone;
                return zone;
            } catch (Exception ex) {
                Logger.LogException(ex);
                return null;
            }
        }

        public void LoadZones()
        {
            try {
                Logger.LogWarning("Loading zones.");
                zones = new Dictionary<string, Zone2D>();
                Hashtable zht = ZoneStore.GetTable("Zones");
                if (zht == null)
                    return;

                foreach (object zone in zht.Values) {
                    var z = zone as SerializedZone2D;
                    if (z == null)
                        continue;
                    Logger.LogWarning("Zone found with name: " + z.Name);
                    z.ToZone2D();
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        public void SaveZones()
        {
            try {
                Logger.LogWarning("Saving " + zones.Count.ToString() + " zone.");
                foreach (var zone in zones.Values) {
                    ZoneStore.Add("Zones", zone.Name, zone.Serialize());
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        public void ChangeTriggerRadius(TriggerBase trigger, float newRadius)
        {
            if (newRadius < 0f) {
                throw new InvalidOperationException(String.Format("Radius can't be less then zero. ChangeTriggerRadius({0}, {1})", trigger, newRadius));
            }

            trigger.GetComponent<SphereCollider>().radius = newRadius;
            trigger.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
        }

        public void ConsoleLog(string str, bool adminOnly = false)
        {
            try {
                foreach (Player player in Server.GetInstance().Players.Values) {
                    if (!adminOnly || (adminOnly && player.Admin))
                        player.ConsoleMessage(str);
                }
            } catch (Exception ex) {
                Logger.LogDebug("ConsoleLog ex");
                Logger.LogException(ex);
            }
        }

        public LoadOut CreateLoadOut(string name)
        {
            return new LoadOut(name);
        }

        /*public StructureRecorder.Structure CreateStructure(string name)
        {
            return new Pluton.StructureRecorder.Structure(name);
        }*/

        public void DestroyEntity(BaseEntity ent)
        {
            ent.GetComponent<BaseNetworkable>().KillMessage();
        }

        public void DestroyEntityGib(BaseEntity ent)
        {
            ent.GetComponent<BaseNetworkable>().Kill(BaseNetworkable.DestroyMode.Gib);
        }

        public void DestroyObject(GameObject go)
        {
            UnityEngine.Object.DestroyImmediate(go);
        }

        // Dumper methods
        public bool DumpObjToFile(string path, object obj, string prefix = "")
        {
            return DumpObjToFile(path, obj, 1, 30, false, false, prefix);
        }

        public bool DumpObjToFile(string path, object obj, int depth, string prefix = "")
        {
            return DumpObjToFile(path, obj, depth, 30, false, false, prefix);
        }

        public bool DumpObjToFile(string path, object obj, int depth, int maxItems, string prefix = "")
        {
            return DumpObjToFile(path, obj, depth, maxItems, false, false, prefix);
        }

        public bool DumpObjToFile(string path, object obj, int depth, int maxItems, bool disPrivate, string prefix = "")
        {
            return DumpObjToFile(path, obj, depth, maxItems, disPrivate, false, prefix);
        }

        public bool DumpObjToFile(string path, object obj, int depth, int maxItems, bool disPrivate, bool fullClassName, string prefix = "")
        {
            path = DataStore.GetInstance().RemoveChars(path);
            path = Path.Combine(UtilPath.FullName, path + ".dump");
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

        public static string NormalizePath(string path)
        {
            string normal = path.Replace(@"\\", @"\").Replace(@"//", @"/").Trim();
            return normal;
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return Path.Combine(GetPublicFolder(), fileName);
        }

        public static string GetIdentityFolder()
        {
            return Path.Combine(GetRootFolder(), Path.Combine("server", ConVar.Server.identity));
        }

        public static string GetLoadoutFolder()
        {
            return Path.Combine(GetPublicFolder(), "LoadOuts");
        }

        public static string GetPluginsFolder()
        {
            return Path.Combine(GetPublicFolder(), "Plugins");
        }

        public static string GetPublicFolder()
        {
            return Path.Combine(GetIdentityFolder(), "Pluton");
        }

        public static string GetRootFolder()
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
        }

        public static string GetServerFolder()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))), "RustDedicated_Data");
        }

        public static string GetStructuresFolder()
        {
            return Path.Combine(GetPublicFolder(), "Structures");
        }

        public void Initialize()
        {
            UtilPath = new DirectoryInfo(Path.Combine(GetPublicFolder(), "Util"));
            ZoneStore = new DataStore("Zones.ds");
            ZoneStore.Load();
            LoadZones();
        }

        public bool CheckDependencies()
        {
            return true;
        }

        [Obsolete("Util.GetUtil() is obsolete, use Util.GetInstance() instead.", false)]
        public static Util GetUtil()
        {
            return Instance;
        }

        public float GetVectorsDistance(Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2);
        }

        public string[] GetQuotedArgs(string[] sArr)
        {
            bool inQuote = false;
            string current = "";
            List<string> final = new List<string>();

            foreach (string str in sArr) {
                if (str.StartsWith("\""))
                    inQuote = true;

                if (str.EndsWith("\""))
                    inQuote = false;

                if (inQuote) {
                    if (current != "")
                        current += " " + str;
                    if (current == "")
                        current = str;
                }

                if (!inQuote) {
                    if (current != "")
                        final.Add((current + " " + str).Replace("\"", ""));
                    if (current == "")
                        final.Add(str.Replace("\"", ""));
                    current = "";
                }
            }
            return final.ToArray();
        }

        public static Hashtable HashtableFromFile(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open)) {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Hashtable)formatter.Deserialize(stream);
            }
        }

        public static void HashtableToFile(Hashtable ht, string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, ht);
            }
        }

        public Vector3 Infront(Player p, float length)
        {
            return (p.Location + ((Vector3)(Vector3.forward * length)));
        }

        public void Items()
        {
            var path = Path.Combine(GetPublicFolder(), "Items.ini");
            if (!File.Exists(path))
                File.AppendAllText("", path);
            var ini = new IniParser(path);
            foreach (ItemDefinition item in ItemManager.itemList) {
                ini.AddSetting(item.displayName.english, "itemid", item.itemid.ToString());
                ini.AddSetting(item.displayName.english, "category", item.category.ToString());
                ini.AddSetting(item.displayName.english, "shortname", item.shortname);
                ini.AddSetting(item.displayName.english, "description", item.displayDescription.english);
            }
            ini.Save();
        }

        public bool IsNull(object obj)
        {
            return (obj == null);
        }

        public void Log(string str)
        {
            Logger.Log(str);
        }

        public Match Regex(string input, string match)
        {
            return new System.Text.RegularExpressions.Regex(input).Match(match);
        }

        public Quaternion RotateX(Quaternion q, float angle)
        {
            return (q *= Quaternion.Euler(angle, 0f, 0f));
        }

        public Quaternion RotateY(Quaternion q, float angle)
        {
            return (q *= Quaternion.Euler(0f, angle, 0f));
        }

        public Quaternion RotateZ(Quaternion q, float angle)
        {
            return (q *= Quaternion.Euler(0f, 0f, angle));
        }

        public bool TryFindType(string typeName, out System.Type t)
        {
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

        public System.Type TryFindReturnType(string typeName)
        {
            System.Type t;
            if (this.TryFindType(typeName, out t))
                return t;
            throw new Exception("Type not found " + typeName);
        }
    }
}
