using System;

namespace Pluton.Events
{
    public class GatherEvent : CountedInstance
    {
        public readonly HitInfo _info;
        public readonly Player Gatherer;
        public readonly Entity Resource;
        public readonly string Prefab;
        public readonly uint PrefabID;
        public readonly int Stage;

        public GatherEvent(BaseEntity res, HitInfo info)
        {
            _info = info;
            Resource = new Entity(res);
            Gatherer = Server.GetPlayer(_info.Initiator as BasePlayer);
            Prefab = res.LookupPrefabName();
            PrefabID = res.prefabID;
            BaseResource br = res as BaseResource;
            if (br != null)
                Stage = br.stage;
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
                    if (_info.Initiator != null)
                        return new Entity(_info.Initiator);
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
                    Logger.LogWarning("[GatherEvent] Got an exception instead of the weapon.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }

        public float Proficiency {
            get {
                return _info.resourceGatherProficiency;
            }
            set {
                _info.resourceGatherProficiency = value;
            }
        }
    }
}

