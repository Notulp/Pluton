using System;

namespace Pluton.PlutonUI
{
    public class RectTransform : BaseComponent
    {
        public string anchormax {
            get {
                return _obj.GetString("anchormax", "1.0 1.0");
            }
            set {
                if (_obj.ContainsKey("anchormax"))
                    _obj["anchormax"] = new JSON.Value(value);
                else
                    _obj.Add("anchormax", new JSON.Value(value));
            }
        }

        public string anchormin {
            get {
                return _obj.GetString("anchormin", "0.0 0.0");
            }
            set {
                if (_obj.ContainsKey("anchormin"))
                    _obj["anchormin"] = new JSON.Value(value);
                else
                    _obj.Add("anchormin", new JSON.Value(value));
            }
        }

        public string offsetmax {
            get {
                return _obj.GetString("offsetmax", "1.0 1.0");
            }
            set {
                if (_obj.ContainsKey("offsetmax"))
                    _obj["offsetmax"] = new JSON.Value(value);
                else
                    _obj.Add("offsetmax", new JSON.Value(value));
            }
        }

        public string offsetmin {
            get {
                return _obj.GetString("offsetmin", "0.0 0.0");
            }
            set {
                if (_obj.ContainsKey("offsetmin"))
                    _obj["offsetmin"] = new JSON.Value(value);
                else
                    _obj.Add("offsetmin", new JSON.Value(value));
            }
        }

        public override string type {
            get {
                return "RectTransform";
            }
        }

        public RectTransform()
        {
            this["type"] = new JSON.Value(type);
        }
    }
}

