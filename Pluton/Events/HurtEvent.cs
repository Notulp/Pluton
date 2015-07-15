using System;

namespace Pluton.Events
{
    public class HurtEvent : CountedInstance
    {
        public readonly HitInfo _info;
        public readonly string HitBone;

        public HurtEvent(HitInfo info)
        {
            this._info = info;
            string bonename = StringPool.Get(info.HitBone);
            this.HitBone = bonename == "" ? "unknown" : bonename;
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

        public float[] DamageAmounts
        {
            get
            {
                return this._info.damageTypes.types;
            }
            set
            {
                this._info.damageTypes.types = value;
            }
        }

        public Rust.DamageType DamageType
        {
            get { return this._info.damageTypes.GetMajorityDamageType(); }
        }

        public Entity Attacker {
            get {
                try {
                    if (_info.Initiator != null) {
                        BaseEntity ent = _info.Initiator;
                        BasePlayer p = ent.GetComponent<BasePlayer>();
                        if (p != null)
                            return Server.GetPlayer(p);

                        BaseNPC n = ent.GetComponent<BaseNPC>();
                        if (n != null)
                            return new NPC(n);

                        return new Entity(ent);
                    }
                    return null;
                } catch (Exception ex) {
                    Logger.LogWarning("[HurtEvent] Got an exception instead of the attacker.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }

        public InvItem Weapon {
            get {
                try {
                    if (_info.Weapon == null)
                        return null;
                    uint itemUID = (uint)_info.Weapon.GetFieldValue("ownerItemUID");

                    BasePlayer ownerPlayer = _info.Weapon.ownerPlayer;
                    if (ownerPlayer == null) {
                        return null;
                    }

                    return new InvItem(ownerPlayer.inventory.FindItemUID(itemUID));
                } catch (Exception ex) {
                    Logger.LogWarning("[HurtEvent] Got an exception instead of the weapon.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }
    }
}

