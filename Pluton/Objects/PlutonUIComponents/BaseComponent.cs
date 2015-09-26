
namespace Pluton.PlutonUI
{
    public class BaseComponent : CountedInstance, IComponent
    {
        public JSON.Value this[string key] {
            get {
                return _obj[key];
            }
            set {
                _obj[key] = value;
            }
        }

        public float fadeIn {
            get {
                return _obj.GetFloat("fadeIn");
            }
            set {
                if (_obj.ContainsKey("fadeIn"))
                    _obj["fadeIn"] = new JSON.Value(value);
                else
                    _obj.Add("fadeIn", new JSON.Value(value));
            }
        }

        protected JSON.Object _obj = new JSON.Object();

        public JSON.Object obj {
            get {
                return _obj;
            }
            set {
                _obj = value;
            }
        }

        public virtual string type { get { return ""; } }
    }
}

