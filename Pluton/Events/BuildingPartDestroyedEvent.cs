using System;

namespace Pluton.Events
{
    public class BuildingPartDestroyedEvent : CountedInstance
    {
        private BuildingPart buildingPart;
        public readonly HitInfo _info;
        public readonly string HitBone;

        public BuildingPartDestroyedEvent(BuildingBlock bb, HitInfo info)
        {
            this._info = info;
            this.buildingPart = new BuildingPart(bb);
            string bonename = StringPool.Get(info.HitBone);
            this.HitBone = bonename == "" ? "unknown" : bonename;
        }

        public BuildingPart BuildingPart
        {
            get { return this.buildingPart; }
        }

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

        public Entity Attacker
        {
            get
            {
                try
                {
                    if (_info.Initiator != null)
                    {
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
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("[HurtEvent] Got an exception instead of the attacker.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }

        public InvItem Weapon
        {
            get
            {
                try
                {
                    if (_info.Weapon == null)
                        return null;
                    uint itemUID = (uint)_info.Weapon.GetFieldValue("ownerItemUID");

                    BasePlayer ownerPlayer = _info.Weapon.ownerPlayer;
                    if (ownerPlayer == null)
                    {
                        return null;
                    }

                    return new InvItem(ownerPlayer.inventory.FindItemUID(itemUID));
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("[HurtEvent] Got an exception instead of the weapon.");
                    Logger.LogException(ex);
                    return null;
                }
            }
        }
    }
}