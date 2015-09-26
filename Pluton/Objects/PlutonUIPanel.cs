using System;

namespace Pluton
{
    public class PlutonUIPanel
    {
        public JSON.Object obj = new JSON.Object();

        public JSON.Value this[string key] {
            get {
                return obj[key];
            }
            set {
                obj[key] = value;
            }
        }

        public JSON.Array components {
            get {
                return obj.GetArray("components");
            }
            set {
                if (obj.ContainsKey("components"))
                    obj["components"] = new JSON.Value(value);
                else
                    obj.Add("components", new JSON.Value(value));
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

        public PlutonUIPanel(string nam = null, string par = null, float? fade = null)
        {
            components = new JSON.Array();
            if (nam != null)
                name = nam;
            if (par != null)
                parent = par;
            if (fade != null)
                fadeOut = (float)fade;
        }

        public PlutonUI.BaseComponent AddComponent(PlutonUI.BaseComponent comp)
        {
            components.Add(comp.obj);
            return comp;
        }

        public PlutonUI.BaseComponent AddComponent <T>() where T : PlutonUI.BaseComponent
        {
            PlutonUI.BaseComponent t = Activator.CreateInstance<T>();
            components.Add(t.obj);
            return t;
        }

        public bool RemoveComponent(PlutonUI.BaseComponent comp)
        {
            for (int i = 0; i < components.Length; i++) {
                if (components[i].Obj == comp.obj) {
                    components.Remove(i);
                    return true;
                }
            }
            return false;
        }
    }
}

