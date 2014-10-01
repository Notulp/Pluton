using System;

namespace Pluton.Events
{
    public class HurtEvent
    {
        private HitInfo _info;

        public HurtEvent(HitInfo info)
        {
            _info = info;
        }

        public float DamageAmount {
            get {
                return _info.damageAmount;
            }
        }

        public string DamageType {
            get {
                return _info.damageType.ToString();
            }
        }
    }
}

