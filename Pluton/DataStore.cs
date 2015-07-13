namespace Pluton
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    public class DataStore : CountedInstance
    {
        public readonly Hashtable datastore;
        static DataStore instance;
        public string PATH;

        static object StringifyIfVector3(object keyorval)
        {
            if (keyorval == null)
                return keyorval;

            try {
                if (typeof(Vector3).Equals(keyorval.GetType())) {
                    return "Vector3," +
                    ((Vector3)keyorval).x.ToString("G9") + "," +
                    ((Vector3)keyorval).y.ToString("G9") + "," +
                    ((Vector3)keyorval).z.ToString("G9");
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
            return keyorval;
        }

        static object ParseIfVector3String(object keyorval)
        {
            if (keyorval == null)
                return keyorval;

            try {
                if (typeof(string).Equals(keyorval.GetType())) {
                    if ((keyorval as string).StartsWith("Vector3,", StringComparison.CurrentCulture)) {
                        string[] v3array = (keyorval as string).Split(',');
                        var parse = new Vector3(Single.Parse(v3array[1]), 
                                            Single.Parse(v3array[2]),
                                            Single.Parse(v3array[3]));
                        return parse;
                    }
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
            }          
            return keyorval;
        }

        public string RemoveChars(string str)
        {
            foreach (string c in new [] { "/", "\\", "%", "$" }) {
                str = str.Replace(c, "");
            }
            return str;
        }

        public bool ToIni(string inifilename = "DataStore")
        {
            string inipath = Path.Combine(Util.GetPublicFolder(), RemoveChars(inifilename).Trim() + ".ini");
            File.WriteAllText(inipath, "");
            var ini = new IniParser(inipath);
            ini.Save();

            foreach (string section in datastore.Keys) {
                var ht = (Hashtable)datastore[section];
                foreach (object setting in ht.Keys) {
                    try {
                        string key = "NullReference";
                        string val = "NullReference";
                        if (setting != null) {
                            if (setting.GetType().GetMethod("ToString", Type.EmptyTypes) == null) {
                                key = "type:" + setting.GetType();
                            } else {
                                key = setting.ToString();
                            }
                        }

                        if (ht[setting] != null) {
                            if (ht[setting].GetType().GetMethod("ToString", Type.EmptyTypes) == null) {
                                val = "type:" + ht[setting].GetType();
                            } else {
                                val = ht[setting].ToString();
                            }            
                        }

                        ini.AddSetting(section, key, val);
                    } catch (Exception ex) {
                        Logger.LogException(ex);
                    }
                }
            }
            ini.Save();
            return true;           
        }

        public void Add(string tablename, object key, object val)
        {
            if (key == null)
                key = "NullReference";

            var hashtable = (Hashtable)datastore[tablename];
            if (hashtable == null) {
                hashtable = new Hashtable();
                datastore.Add(tablename, hashtable);
            }
            hashtable[StringifyIfVector3(key)] = StringifyIfVector3(val);
        }

        public bool ContainsKey(string tablename, object key)
        {
            if (key == null)
                return false;

            var hashtable = (Hashtable)datastore[tablename];
            return hashtable != null && hashtable.ContainsKey(StringifyIfVector3(key));
        }

        public bool ContainsValue(string tablename, object val)
        {
            var hashtable = (Hashtable)datastore[tablename];
            return hashtable != null && hashtable.ContainsValue(StringifyIfVector3(val));
        }

        public int Count(string tablename)
        {
            var hashtable = (Hashtable)datastore[tablename];
            return hashtable == null ? 0 : hashtable.Count;
        }

        public void Flush(string tablename)
        {
            if (((Hashtable)datastore[tablename]) != null) {
                datastore.Remove(tablename);
            }
        }

        public object Get(string tablename, object key)
        {
            if (key == null)
                return null;

            var hashtable = (Hashtable)datastore[tablename];
            return hashtable == null ? null : ParseIfVector3String(hashtable[StringifyIfVector3(key)]);
        }

        public static DataStore GetInstance()
        {
            if (instance == null) {
                instance = new DataStore("PlutonDatastore.ds");
            }
            return instance;
        }

        public Hashtable GetTable(string tablename)
        {
            var hashtable = (Hashtable)datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            var parse = new Hashtable(hashtable.Count);
            foreach (DictionaryEntry entry in hashtable) {
                parse.Add(ParseIfVector3String(entry.Key), ParseIfVector3String(entry.Value));
            }
            return parse;
        }

        public object[] Keys(string tablename)
        {
            var hashtable = (Hashtable)datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            var parse = new List<object>(hashtable.Keys.Count);
            foreach (object key in hashtable.Keys) {
                parse.Add(ParseIfVector3String(key));
            }
            return parse.ToArray<object>();
        }

        public void Load()
        {
            if (File.Exists(PATH)) {
                try {
                    Hashtable hashtable = Util.HashtableFromFile(PATH);

                    datastore.Clear();
                    int count = 0;
                    foreach (DictionaryEntry entry in hashtable) {
                        datastore[entry.Key] = entry.Value;
                        count += (entry.Value as Hashtable).Count;
                    }

                    Debug.Log("DataStore Loaded from " + PATH);
                    Debug.Log(String.Format("Tables: {0}! Keys: {1}", datastore.Count, count));
                } catch (Exception ex) {
                    Logger.LogException(ex);
                }
            }
        }

        public void Remove(string tablename, object key)
        {
            if (key == null)
                return;

            var hashtable = (Hashtable)datastore[tablename];
            if (hashtable != null) {
                hashtable.Remove(StringifyIfVector3(key));
            }
        }

        public void Save()
        {
            if (datastore.Count != 0) {
                Util.HashtableToFile(datastore, PATH);
                Debug.Log("DataStore saved to " + PATH);
            }
        }

        public object[] Values(string tablename)
        {
            var hashtable = (Hashtable)datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            var parse = new List<object>(hashtable.Values.Count);
            foreach (object val in hashtable.Values) {
                parse.Add(ParseIfVector3String(val));
            }
            return parse.ToArray();
        }

        public DataStore(string path)
        {
            path = RemoveChars(path);
            datastore = new Hashtable();
            PATH = Path.Combine(Util.GetPublicFolder(), path);
            Debug.Log("New DataStore instance: " + PATH);
        }
    }
}
