using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pluton{

    public class IniParser : CountedInstance
    {
        readonly string iniFilePath;
        readonly Hashtable keyPairs = new Hashtable();
        readonly List<SectionPair> tmpList = new List<SectionPair>();

        public readonly string Name;

        public IniParser(string iniPath)
        {
            string str2 = null;
            iniFilePath = iniPath;
            Name = Path.GetFileNameWithoutExtension(iniPath);

            if (!File.Exists(iniPath))
                throw new FileNotFoundException("Unable to locate " + iniPath);

            using (TextReader reader = new StreamReader(iniPath)) {
                for (string str = reader.ReadLine(); str != null; str = reader.ReadLine()) {
                    str = str.Trim();
                    if (str == "")
                        continue;

                    if (str.StartsWith("[", StringComparison.Ordinal) && str.EndsWith("]", StringComparison.Ordinal))
                        str2 = str.Substring(1, str.Length - 2);
                    else {
                        SectionPair pair;

                            if (str.StartsWith(";", StringComparison.Ordinal))
                                str = str.Replace("=", "%eq%") + @"=%comment%";

                        string[] strArray = str.Split(new char[] { '=' }, 2);
                        string str3 = null;
                        str2 = str2 ?? "ROOT";
                        pair.Section = str2;
                        pair.Key = strArray[0];
                        if (strArray.Length > 1) {
                            str3 = strArray[1];
                        }
                        keyPairs.Add(pair, str3);
                        tmpList.Add(pair);
                    }
                }
            }
        }

        public void AddSetting(string sectionName, string settingName)
        {
            AddSetting(sectionName, settingName, String.Empty);
        }

        public void AddSetting(string sectionName, string settingName, string settingValue)
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            if (keyPairs.ContainsKey(pair)) {
                keyPairs.Remove(pair);
            }
            if (tmpList.Contains(pair)) {
                tmpList.Remove(pair);
            }
            keyPairs.Add(pair, settingValue);
            tmpList.Add(pair);
        }

        public int Count()
        {
            return Sections.Length;
        }

        public void DeleteSetting(string sectionName, string settingName)
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            if (keyPairs.ContainsKey(pair)) {
                keyPairs.Remove(pair);
                tmpList.Remove(pair);
            }
        }

        public string[] EnumSection(string sectionName)
        {
            var list = new List<string>();
            foreach (SectionPair pair in tmpList) {
                if (pair.Key.StartsWith(";", StringComparison.Ordinal))
                    continue;

                if (pair.Section == sectionName) {
                    list.Add(pair.Key);
                }
            }
            return list.ToArray();
        }

        public string[] Sections {
            get {
                return (from pair in tmpList
                        orderby pair.Section ascending
                        select pair.Section).ToArray<string>();
            }
        }

        public string GetSetting(string sectionName, string settingName, string defaultValue = "")
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            if (!keyPairs.ContainsKey(pair)) {
                AddSetting(sectionName, settingName, defaultValue);
                return defaultValue;
            }
            return ((string)keyPairs[pair]).Trim();
        }

        public bool GetBoolSetting(string sectionName, string settingName, bool defaultValue = false)
        {
            return defaultValue ? (GetSetting(sectionName, settingName, "true").ToLower() != "false") : (GetSetting(sectionName, settingName, "false").ToLower() == "true");
        }

        public bool isCommandOn(string cmdName)
        {
            string setting = GetSetting("Commands", cmdName);
            return ((setting == null) || (setting == "true"));
        }

        public void Save()
        {
            SaveSettings(iniFilePath);
        }

        public void SaveSettings(string newFilePath)
        {
            var list = new ArrayList();
            string str = "";
            string str2 = "";
            foreach (SectionPair pair in tmpList) {
                if (!list.Contains(pair.Section)) {
                    list.Add(pair.Section);
                }
            }
            foreach (string str3 in list) {
                str2 = str2 + "[" + str3 + "]\r\n";
                foreach (SectionPair pair2 in tmpList) {
                    if (pair2.Section == str3) {
                        str = (string)keyPairs[pair2];
                        if (str != null) {
                            if (str == "%comment%") {
                                str = "";
                            } else {
                                str = "=" + str;
                            }
                        }
                        str2 = str2 + pair2.Key.Replace("%eq%", "=") + str + "\r\n";
                    }
                }
                str2 = str2 + "\r\n";
            }

            using (TextWriter writer = new StreamWriter(newFilePath))
                writer.Write(str2);
        }

        public void SetSetting(string sectionName, string settingName, string value)
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            if (keyPairs.ContainsKey(pair)) {
                keyPairs[pair] = value;
            } else {
                AddSetting(sectionName, settingName, value);
            }
        }

        public bool ContainsSetting(string sectionName, string settingName)
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            return keyPairs.Contains(pair);
        }
        
        public bool ContainsValue(string valueName)
        {
            return keyPairs.ContainsValue(valueName);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SectionPair
        {
            public string Section;
            public string Key;
        }
    }
}
