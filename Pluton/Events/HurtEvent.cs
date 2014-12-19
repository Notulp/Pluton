using System;

namespace Pluton.Events
{
    public class HurtEvent : CountedInstance
    {
        public readonly HitInfo _info;
        public readonly string HitBone;

        public HurtEvent(HitInfo info)
        {
            _info = info;
            string bonename = StringPool.Get(info.HitBone);
            HitBone = bonename==""?"unknown":bonename;
        }

        public float[] DamageAmounts {
            get {
                return _info.damageTypes.types;
            }
            set {
                _info.damageTypes.types = value;
            }
        }

        public Rust.DamageType DamageType {
            get {
                return _info.damageTypes.GetMajorityDamageType();
            }
        }

        public BaseEntity Attacker {
            get {
                return _info.Initiator;
            }
        }

        public InvItem Weapon {
            get {
                try {
                    if (_info.Weapon == null)
                        return null;
                    if (_info.Weapon.GetItem() == null)
                        return null;

                    return new InvItem(_info.Weapon.GetItem());
                } catch (Exception ex) {
                    Logger.LogWarning("[HurtEvent] Got an exception instead of the weapon.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }
    }
}

