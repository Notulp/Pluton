using System;

namespace Pluton.Events
{
    public class GatherEvent : CountedInstance
    {
        public readonly HitInfo _info;
        public readonly Player Gatherer;
        public readonly BaseResource _res;
        public readonly string Prefab;
        public readonly uint PrefabID;
        public readonly InvItem Tool;
        public readonly int Stage;

        public GatherEvent(BaseResource res, HitInfo info)
        {
            _info = info;
            _res = res;
            Gatherer = Server.GetPlayer(_info.Initiator as BasePlayer);
            Prefab = _res.LookupPrefabName();
            PrefabID = _res.prefabID;
            Stage = _res.stage;
            if (_info.Weapon != null)
                Tool = new InvItem(_info.Weapon);

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

