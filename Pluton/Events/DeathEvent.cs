﻿using System;

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
            HitBone = bonename == "" ? "unknown" : bonename;
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

        public Entity Attacker {
            get {
                try {
                    if (_info.Initiator != null) {
                        BaseEntity ent = _info.Initiator;
                        BasePlayer p = ent.GetComponent<BasePlayer>();
                        if (p != null)
                            return Server.GetPlayer(p);

                        BaseNPC n = ent.GetComponent<BaseNPC>();
                        return n != null ? new NPC(n) : new Entity(ent);

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
                    return ownerPlayer == null ? null : new InvItem(ownerPlayer.inventory.FindItemUID(itemUID));

                } catch (Exception ex) {
                    Logger.LogWarning("[DeathEvent] Got an exception instead of the weapon.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }
    }
}

