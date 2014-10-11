namespace Pluton
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    public class DataStore
    {
        public readonly Hashtable datastore;
        private static DataStore instance;
        public string PATH;

        private object StringifyIfVector3(object keyorval)
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

        private object ParseIfVector3String(object keyorval)
        {
            if (keyorval == null)
                return keyorval;

            try {
                if (typeof(string).Equals(keyorval.GetType())) {
                    if ((keyorval as string).StartsWith("Vector3,")) {
                        string[] v3array = (keyorval as string).Split(new char[] { ',' });
                        Vector3 parse = new Vector3(Single.Parse(v3array[1]), 
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
            foreach (string c in new string[] { "/", "\\", "%", "$" }) {
                str = str.Replace(c, "");
            }
            return str;
        }

        public bool ToIni(string inifilename = "DataStore")
        {
            string inipath = Path.Combine(Util.GetPublicFolder(), RemoveChars(inifilename).Trim() + ".ini");
            File.WriteAllText(inipath, "");
            IniParser ini = new IniParser(inipath);
            ini.Save();

            foreach (string section in this.datastore.Keys) {
                Hashtable ht = (Hashtable)this.datastore[section];
                foreach (object setting in ht.Keys) {
                    try {
                        string key = "NullReference";
                        string val = "NullReference";
                        if (setting != null) {
                            if (setting.GetType().GetMethod("ToString", Type.EmptyTypes) == null) {
                                key = "type:" + setting.GetType().ToString();
                            } else {
                                key = setting.ToString();
                            }
                        }

                        if (ht[setting] != null) {
                            if (ht[setting].GetType().GetMethod("ToString", Type.EmptyTypes) == null) {
                                val = "type:" + ht[setting].GetType().ToString();
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

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                hashtable = new Hashtable();
                this.datastore.Add(tablename, hashtable);
            }
            hashtable[StringifyIfVector3(key)] = StringifyIfVector3(val);
        }

        public bool ContainsKey(string tablename, object key)
        {
            if (key == null)
                return false;

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null) {
                return hashtable.ContainsKey(StringifyIfVector3(key));
            }
            return false;
        }

        public bool ContainsValue(string tablename, object val)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null) {
                return hashtable.ContainsValue(StringifyIfVector3(val));
            }
            return false;
        }

        public int Count(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return 0;
            }
            return hashtable.Count;
        }

        public void Flush(string tablename)
        {
            if (((Hashtable)this.datastore[tablename]) != null) {
                this.datastore.Remove(tablename);
            }
        }

        public object Get(string tablename, object key)
        {
            if (key == null)
                return null;

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            return ParseIfVector3String(hashtable[StringifyIfVector3(key)]);
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
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            Hashtable parse = new Hashtable(hashtable.Count);
            foreach (DictionaryEntry entry in hashtable) {
                parse.Add(ParseIfVector3String(entry.Key), ParseIfVector3String(entry.Value));
            }
            return parse;
        }

        public object[] Keys(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            List<object> parse = new List<object>(hashtable.Keys.Count);
            foreach (object key in hashtable.Keys) {
                parse.Add(ParseIfVector3String(key));
            }
            return parse.ToArray<object>();
        }

        public void Load()
        {
            if (File.Exists(this.PATH)) {
                try {
                    Hashtable hashtable = Util.HashtableFromFile(this.PATH);

                    this.datastore.Clear();
                    int count = 0;
                    foreach (DictionaryEntry entry in hashtable) {
                        this.datastore[entry.Key] = entry.Value;
                        count += (entry.Value as Hashtable).Count;
                    }

                    Debug.Log("DataStore Loaded from " + this.PATH);
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

            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable != null) {
                hashtable.Remove(StringifyIfVector3(key));
            }
        }

        public void Save()
        {
            if (this.datastore.Count != 0) {
                Util.HashtableToFile(this.datastore, this.PATH);
                Debug.Log("DataStore saved to " + this.PATH);
            }
        }

        public object[] Values(string tablename)
        {
            Hashtable hashtable = (Hashtable)this.datastore[tablename];
            if (hashtable == null) {
                return null;
            }
            List<object> parse = new List<object>(hashtable.Values.Count);
            foreach (object val in hashtable.Values) {
                parse.Add(ParseIfVector3String(val));
            }
            return parse.ToArray();
        }

        public DataStore(string path)
        {
            path = RemoveChars(path);
            datastore = new Hashtable();
            this.PATH = Path.Combine(Util.GetPublicFolder(), path);
            Debug.Log("New DataStore instance: " + this.PATH);
        }
    }
}