using System;

namespace Pluton.PlutonUI
{
    public class Text : BaseComponent
    {
        public override string type {
            get {
                return "UnityEngine.UI.Text";
            }
        }

        public string align {
            get {
                return _obj.GetString("align", "UpperLeft");
            }
            set {
                if (_obj.ContainsKey("align"))
                    _obj["align"] = new JSON.Value(value);
                else
                    _obj.Add("align", new JSON.Value(value));
            }
        }

        public string color {
            get {
                return _obj.GetString("color", "1.0 1.0 1.0 1.0");
            }
            set {
                if (_obj.ContainsKey("color"))
                    _obj["color"] = new JSON.Value(value);
                else
                    _obj.Add("color", new JSON.Value(value));
            }
        }

        public string font {
            get {
                return _obj.GetString("font", "RobotoCondensed-Bold.ttf");
            }
            set {
                if (_obj.ContainsKey("font"))
                    _obj["font"] = new JSON.Value(value);
                else
                    _obj.Add("font", new JSON.Value(value));
            }
        }

        public int fontSize {
            get {
                return _obj.GetInt("fontSize", 14);
            }
            set {
                if (_obj.ContainsKey("fontSize"))
                    _obj["fontSize"] = new JSON.Value(value);
                else
                    _obj.Add("fontSize", new JSON.Value(value));
            }
        }

        public string text {
            get {
                return _obj.GetString("text", "emptytext");
            }
            set {
                if (_obj.ContainsKey("text"))
                    _obj["text"] = new JSON.Value(value);
                else
                    _obj.Add("text", new JSON.Value(value));
            }
        }
    }
}

