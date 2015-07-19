using System;

namespace Pluton.PlutonUI
{
    public class Outline : BaseComponent
    {
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

        public string distance {
            get {
                return _obj.GetString("distance", "1.0 -1.0");
            }
            set {
                if (_obj.ContainsKey("distance"))
                    _obj["distance"] = new JSON.Value(value);
                else
                    _obj.Add("distance", new JSON.Value(value));
            }
        }

        public override string type {
            get {
                return "UnityEngine.UI.Outline";
            }
        }

        public bool useGraphicAlpha {
            get {
                return _obj.ContainsKey("useGraphicAlpha");
            }
            set {
                if (_obj.ContainsKey("useGraphicAlpha") && !value)
                    _obj.Remove("useGraphicAlpha");
                else if (!_obj.ContainsKey("useGraphicAlpha") && value)
                    _obj.Add("useGraphicAlpha", new JSON.Value(value));
            }
        }
    }
}

