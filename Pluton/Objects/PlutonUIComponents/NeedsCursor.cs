using System;

namespace Pluton.PlutonUI
{
    public class NeedsCursor : BaseComponent
    {
        public override string type {
            get {
                return "NeedsCursor";
            }
        }

        public NeedsCursor()
        {
            this["type"] = new JSON.Value(type);
        }
    }
}

