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

    public class Util
    {
        private readonly Dictionary<string, System.Type> typeCache = new Dictionary<string, System.Type>();
        private static Util util;
        private static DirectoryInfo UtilPath;

        public Dictionary<string, GameObject> zones = new Dictionary<string, GameObject>();
        public DataStore ZoneStore;

        public Zone2D GetZone(string name)
        {
            return zones[name].GetComponent<Zone2D>();
        }

        public void SetZone(Zone2D zone)
        {
            zones[zone.Name] = zone.gameObject;
        }

        public Zone2D CreateZone(string name)
        {
            try {
                GameObject obj = new GameObject(name);
                var gobj = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
                var zone = gobj.AddComponent<Zone2D>();
                zone.Name = name;
                zones[name] = zone.gameObject;
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
                Hashtable zht = ZoneStore.GetTable("Zones");
                foreach (object zone in zht.Values) {
                    var z = zone as SerializedZone2D;
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
                    var z = zone.gameObject.GetComponent<Zone2D>();
                    ZoneStore.Add("Zones", z.Name, z.Serialize());
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        public void ConsoleLog(string str, bool adminOnly = false)
        {
            try
            {
                foreach (Player player in Server.GetServer().Players.Values)
                {
                    if (!adminOnly || (adminOnly && player.Admin))
                        player.ConsoleMessage(str);
                }
            }
            catch (Exception ex)
            {
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
            ent.GetComponent<BaseNetworkable>().Kill();
        }

        public void DestroyEntityGib(BaseEntity ent)
        {
            ent.GetComponent<BaseNetworkable>().Kill(ProtoBuf.EntityDestroy.Mode.Gib, 2, 1, ent.transform.position);
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
            return Path.Combine(GetRootFolder(), Path.Combine("server", server.identity));
        }

        public static string GetLoadoutFolder()
        {
            return Path.Combine(GetPublicFolder(), "LoadOuts");
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

        public static Util GetUtil()
        {
            if (util == null) {
                util = new Util();
                UtilPath = new DirectoryInfo(Path.Combine(GetPublicFolder(), "Util"));
                util.ZoneStore = new DataStore("Zones.ds");
                util.LoadZones();
            }
            return util;
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
            foreach (Item.Definition item in ItemManager.Instance.itemList) {
                ini.AddSetting(item.displayname, "itemid", item.itemid.ToString());
                ini.AddSetting(item.displayname, "category", item.category);
                ini.AddSetting(item.displayname, "shortname", item.shortname);
                ini.AddSetting(item.displayname, "worldPrefab", item.worldprefab);
                ini.AddSetting(item.displayname, "description", item.description);
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