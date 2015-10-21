using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pluton
{
    public class IniParser : Pluton.CountedInstance
    {
        public readonly string FilePath;

        public readonly string Name;

        public Dictionary<string, IniSection> Sections = new Dictionary<string, IniSection>();

        public string this[string section, string setting] {
            get {
                return this[section][setting];
            }
            set {
                if (Sections.ContainsKey(section)) {
                    if (Sections[section].Settings.ContainsKey(setting)) {
                        Sections[section].Settings[setting].Value = value;
                    } else {
                        Sections[section].Settings.Add(setting, new IniSetting(setting, value));
                    }
                } else {
                    Sections.Add(section, new IniSection(section));
                    Sections[section].Settings.Add(setting, new IniSetting(setting, value));
                }   
            }
        }

        public IniSection this[string section] {
            get {
                if (Sections.ContainsKey(section))
                    return Sections[section];
                return new IniSection("");
            }
        }

        public IniParser(string iniPath)
        {
            string section = "ROOT";
            FilePath = iniPath;

            IniSection currentSection = null;

            bool inroot = true;

            List<string> comments = new List<string>();

            Name = Path.GetFileNameWithoutExtension(iniPath);

            if (!File.Exists(iniPath))
                throw new FileNotFoundException("Unable to locate " + iniPath);

            
            using (TextReader reader = new StreamReader(iniPath)) {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
                    line = line.Trim();
                    if (line == String.Empty)
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]")) {

                        section = line.Substring(1, line.Length - 2);

                        if (!Sections.ContainsKey(section)) {
                            Sections.Add(section, new IniSection(section));
                            if (comments.Count != 0) {
                                Sections[section].Comments.AddRange(comments);
                                comments = new List<string>();
                            }
                        }
                        currentSection = Sections[section];
                        inroot = false;
                    } else {
                        if (line.StartsWith(";")) {
                            comments.Add(line);
                            continue;
                        }

                        string[] ConfEqValue = line.Split(new char[] { '=' }, 2);

                        if (ConfEqValue.Length == 0)
                            continue;

                        if (inroot) {
                            if (!Sections.ContainsKey(section)) {
                                Sections.Add(section, new IniSection(section));
                                if (comments.Count != 0) {
                                    Sections[section].Comments.AddRange(comments);
                                    comments = new List<string>();
                                }
                            }
                            currentSection = Sections[section];
                            inroot = false;
                        }


                        if (ConfEqValue.Length == 2)
                            currentSection.AddSetting(ConfEqValue[0], ConfEqValue[1]);

                        else if (ConfEqValue.Length == 1)
                            currentSection.AddSetting(ConfEqValue[0], null);

                        if (comments.Count != 0) {
                            currentSection.Settings[ConfEqValue[0]].Comments.AddRange(comments);
                            comments = new List<string>();
                        }
                    }
                }
            }
        }

        public void AddSectionComments(string section, params string[] comments) {
            if (Sections.ContainsKey(section))
                Sections[section].Comments.AddRange(comments);
            else
                Logger.LogWarning($"[IniParser] There is no [{section}] section in: {FilePath}");
        }

        public void AddSettingComments(string section, string setting, params string[] comments) {
            if (Sections.ContainsKey(section) && Sections[section].Settings.ContainsKey(setting))
                Sections[section].Settings[setting].Comments.AddRange(comments);
            else
                Logger.LogWarning($"[IniParser] There is no {setting} setting in [{section}] section in: {FilePath}");
        }

        public void AddSetting(string section, string setting)
        {
            this[section, setting] = String.Empty;
        }

        public void AddSetting(string section, string setting, string value)
        {
            this[section, setting] = value;
        }

        public int Count()
        {
            return Sections.Count;
        }

        public void DeleteSetting(string section, string setting)
        {
            this[section]?.Settings.Remove(setting);
        }

        public void DeleteSection(string section)
        {
            if (Sections.ContainsKey(section))
                Sections.Remove(section);
        }

        public string[] EnumSection(string section)
        {
            return Sections.ContainsKey(section) ? Sections[section].Settings.Keys.ToArray() : new string[0];
        }

        public string GetSetting(string section, string setting)
        {
            return this[section, setting];
        }

        public string GetSetting(string section, string setting, string defaultvalue)
        {
            if (this[section, setting] != null)
                return this[section, setting];
            this[section, setting] = defaultvalue;
            return defaultvalue;
        }

        public bool GetBoolSetting(string section, string setting)
        {
            bool result = false;
            if (Boolean.TryParse(this[section, setting], out result))
                return result;
            Logger.LogWarning($"[IniParser] [{section}] -> {setting} -> {this[section, setting]} cant be converted to Boolean. ({FilePath})");
            return false;
        }

        public bool GetBoolSetting(string section, string setting, bool defaultvalue)
        {
            if (this[section, setting] != null)
                return GetBoolSetting(section, setting);
            this[section, setting] = defaultvalue.ToString();
            return defaultvalue;
        }

        public int GetIntSetting(string section, string setting)
        {
            int result = 0;
            if (Int32.TryParse(this[section, setting], out result))
                return result;
            Logger.LogWarning($"[IniParser] [{section}] -> {setting} -> {this[section, setting]} cant be converted to Int32. ({FilePath})");
            return 0;
        }

        public int GetIntSetting(string section, string setting, int defaultvalue)
        {
            if (this[section, setting] != null)
                return GetIntSetting(section, setting);
            this[section, setting] = defaultvalue.ToString();
            return defaultvalue;
        }

        public void Save()
        {
            this.SaveSettings(FilePath);
        }

        public void SaveSettings(string newFilePath)
        {
            string result = String.Empty;
            foreach (var section in Sections.Values) {
                result += section.ToString() + Environment.NewLine;
            }

            using (TextWriter writer = new StreamWriter(newFilePath))
                writer.Write(result);
        }

        public void SetSetting(string section, string setting, string value)
        {
            this[section, setting] = value;
        }

        public bool ContainsSetting(string section, string setting)
        {
            return Sections.ContainsKey(section) && Sections[section].Settings.ContainsKey(setting);
        }

        public bool ContainsValue(string value)
        {
            return Sections.Values.Any(section => {
                return section.Settings.Any(setting => {
                    return setting.Value.Value == value;
                });
            });
        }

        public class IniSection
        {

            public List<string> Comments = new List<string>();

            public string this[string index] {
                get {
                    if (Settings.ContainsKey(index))
                        return Settings[index].Value;
                    return null;
                }
                set {
                    AddSetting(index, value);
                }
            }

            public string SectionName;
            public Dictionary<string, IniSetting> Settings = new Dictionary<string, IniSetting>();

            public IniSection(string name) {
                SectionName = name;
            }

            public void AddSetting(string setting, string value) {
                if (!Settings.ContainsKey(setting))
                    Settings.Add(setting, new IniSetting(setting, value));
                else
                    Settings[setting] = new IniSetting(setting, value);
            }

            public IniSection(string name, params string[] comments)
                : this(name)
            {
                Comments = comments.ToList();
            }

            public IniSection(string name, IEnumerable<string> comments)
                : this(name)
            {
                Comments = comments.ToList();
            }

            public void AddComment(string comment) {
                Comments.Add(comment);
            }

            public void RemoveComment(string comment) {
                Comments.Remove(comment);
            }

            public void ClearComments() {
                Comments.Clear();
            }

            public override string ToString()
            {
                string result = String.Empty;
                string result2 = String.Empty;

                if (Comments.Count != 0)
                    foreach (string comment in Comments)
                        result += ";" + comment + Environment.NewLine;

                foreach (IniSetting setting in Settings.Values)
                    result2 += setting.ToString();

                return result + $"[{SectionName}]" + Environment.NewLine + result2;
            }
        }

        public class IniSetting {

            public List<string> Comments = new List<string>();

            public string SettingName;
            public string Value;

            public IniSetting(string name, string value) {
                SettingName = name;
                Value = value;
            }

            public IniSetting(string name, string value, params string[] comments)
                : this(name, value)
            {
                Comments = comments.ToList();
            }

            public IniSetting(string name, string value, IEnumerable<string> comments)
                : this(name, value)
            {
                Comments = comments.ToList();
            }

            public void AddComment(string comment) {
                Comments.Add(comment);
            }

            public void RemoveComment(string comment) {
                Comments.Remove(comment);
            }

            public void ClearComments() {
                Comments.Clear();
            }

            public override string ToString()
            {
                string result = String.Empty;
                if (Comments.Count != 0) {
                    result += Environment.NewLine;
                    foreach (string comment in Comments)
                        result += ";" + comment + Environment.NewLine;
                }

                return result + $"{SettingName}={Value}" + Environment.NewLine;
            }
        }
    }
}

