using System;

namespace Pluton.Events
{
    public class DeathEvent : CountedInstance
    {
        public readonly HitInfo _info;
        public bool dropLoot = true;
        public readonly string HitBone;

        public DeathEvent(HitInfo info)
        {
            _info = info;
            string bonename = StringPool.Get(info.HitBone);
            HitBone = bonename==""?"unknown":bonename;
        }

        /******************
        *                 *
        * Generic      0  *
        * Hunger       1  *
        * Thirst       2  *
        * Cold         3  *
        * Drowned      4  *
        * Heat         5  *
        * Bleeding     6  *
        * Poison       7  *
        * Suicide      8  *
        * Bullet       9  *
        * Slash        10 *
        * Blunt        11 *
        * Fall         12 *
        * Radiation    13 *
        * Bite         14 *
        * Stab         15 *
        *                 *
        ******************/

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
                    Logger.LogWarning("[DeathEvent] Got an exception instead of the weapon.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }
    }
}

