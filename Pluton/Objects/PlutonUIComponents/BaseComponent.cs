using System;

namespace Pluton.PlutonUIPanel
{
    public class BaseComponent : CountedInstance, IComponent
    {
        public JSON.Value this [string key] {
            get {
                return _obj[key];
            }
            set {
                _obj[key] = value;
            }
        }

        public virtual string type { get { return ""; }}

        protected JSON.Object _obj = new JSON.Object();
        public JSON.Object obj {
            get {
                return _obj;
            }
        }
    }
}

