using System;

namespace Pluton
{
    public class PlutonUIPanel
    {
        public JSON.Array components = new JSON.Array();
        public JSON.Object obj = new JSON.Object();

        public JSON.Value this [string key] {
            get {
                return obj[key];
            }
            set {
                obj[key] = value;
            }
        }

        public float fadeOut {
            get {
                return obj.GetFloat("fadeOut", 0f);
            }
            set {
                if (obj.ContainsKey("fadeOut"))
                    obj["fadeOut"] = new JSON.Value(value);
                else
                    obj.Add("fadeOut", new JSON.Value(value));
            }
        }

        public string name {
            get {
                return obj.GetString("name", "PlutonUI Panel");
            }
            set {
                if (obj.ContainsKey("name"))
                    obj["name"] = new JSON.Value(value);
                else
                    obj.Add("name", new JSON.Value(value));
            }
        }

        public string parent {
            get {
                return obj.GetString("parent", "Overlay");
            }
            set {
                if (obj.ContainsKey("parent"))
                    obj["parent"] = new JSON.Value(value);
                else
                    obj.Add("parent", new JSON.Value(value));
            }
        }
    }
}

