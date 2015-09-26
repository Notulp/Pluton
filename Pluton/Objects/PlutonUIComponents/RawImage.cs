
namespace Pluton.PlutonUI
{
    public class RawImage : BaseComponent
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

        public string material {
            get {
                return _obj.GetString("material");
            }
            set {
                if (_obj.ContainsKey("material"))
                    _obj["material"] = new JSON.Value(value);
                else
                    _obj.Add("material", new JSON.Value(value));
            }
        }

        public string png {
            get {
                return _obj.GetString("png");
            }
            set {
                if (_obj.ContainsKey("png"))
                    _obj["png"] = new JSON.Value(value);
                else
                    _obj.Add("png", new JSON.Value(value));
            }
        }

        public string sprite {
            get {
                return _obj.GetString("sprite", "Assets/Icons/rust.png");
            }
            set {
                if (_obj.ContainsKey("sprite"))
                    _obj["sprite"] = new JSON.Value(value);
                else
                    _obj.Add("sprite", new JSON.Value(value));
            }
        }

        public override string type {
            get {
                return "UnityEngine.UI.RawImage";
            }
        }

        public string url {
            get {
                return _obj.GetString("url");
            }
            set {
                if (_obj.ContainsKey("url"))
                    _obj["url"] = new JSON.Value(value);
                else
                    _obj.Add("url", new JSON.Value(value));
            }
        }

        public RawImage()
        {
            this["type"] = new JSON.Value(type);
        }
    }
}

