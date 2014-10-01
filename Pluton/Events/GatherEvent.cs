using System;

namespace Pluton.Events
{
    public class GatherEvent
    {
        private HitInfo _info;
        private BaseResource _res;

        public GatherEvent(BaseResource res, HitInfo info)
        {
            _info = info;
            _res = res;
        }

        public float DamageAmount {
            get {
                return _info.damageAmount;
            }
            set {
                _info.damageAmount = value;
            }
        }

        public Rust.DamageType DamageType {
            get {
                return _info.damageType;
            }
        }

        public float Health {
            get {
                return _res.health;
            }
            set {
                _res.health = value;
            }
        }

        public string Prefab {
            get {
                return _res.sourcePrefab;
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

        public Player Gatherer {
            get {
                return new Player(_info.Initiator as BasePlayer);
            }
        }

        public int Stage {
            get {
                return _res.stage;
            }
        }

        public string WeaponName {
            get {
                return _info.Weapon.info.displayname;
            }
        }
    }
}

