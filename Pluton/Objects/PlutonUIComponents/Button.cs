using System;

namespace Pluton.PlutonUI
{
    public class Button : BaseComponent
    {
        public override string type {
            get {
                return "UnityEngine.UI.Button";
            }
        }

        public bool close {
            get {
                return _obj.ContainsKey("close");
            }
            set {
                if (_obj.ContainsKey("close") && !value)
                    _obj.Remove("close");
                else if (!_obj.ContainsKey("close") && value)
                    _obj.Add("close", new JSON.Value(value));
            }
        }

        public string command {
            get {
                return _obj.GetString("command");
            }
            set {
                if (_obj.ContainsKey("command"))
                    _obj["command"] = new JSON.Value(value);
                else
                    _obj.Add("command", new JSON.Value(value));
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

        public string imagetype {
            get {
                return _obj.GetString("imagetype", "Simple");
            }
            set {
                if (_obj.ContainsKey("imagetype"))
                    _obj["imagetype"] = new JSON.Value(value);
                else
                    _obj.Add("imagetype", new JSON.Value(value));
            }
        }

        public string material {
            get {
                return _obj.GetString("material", "Assets/Icons/IconMaterial.mat");
            }
            set {
                if (_obj.ContainsKey("material"))
                    _obj["material"] = new JSON.Value(value);
                else
                    _obj.Add("material", new JSON.Value(value));
            }
        }

        public string sprite {
            get {
                return _obj.GetString("sprite", "Assets/Content/UI/UI.Background.Tile.psd");
            }
            set {
                if (_obj.ContainsKey("sprite"))
                    _obj["sprite"] = new JSON.Value(value);
                else
                    _obj.Add("sprite", new JSON.Value(value));
            }
        }
    }
}

