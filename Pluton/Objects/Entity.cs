using System;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class Entity : CountedInstance
    {
        [NonSerialized]
        public readonly BaseEntity baseEntity;
        public readonly string Prefab;
        public readonly uint PrefabID;

        public Entity(BaseEntity ent)
        {
            baseEntity = ent;
            Prefab = baseEntity.LookupPrefabName();
            PrefabID = baseEntity.prefabID;
        }

        public virtual void Kill()
        {
            baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
        }

        public virtual bool IsBuildingPart()
        {
            return false;
        }

        public virtual bool IsNPC()
        {
            return false;
        }

        public virtual bool IsPlayer()
        {
            return false;
        }

        public BuildingPart ToBuildingPart()
        {
            BuildingBlock b = baseEntity.GetComponent<BuildingBlock>();
            if (b == null)
                return null;
            return new BuildingPart(b);
        }

        public NPC ToNPC()
        {
            BaseNPC a = baseEntity.GetComponent<BaseNPC>();
            if (a == null)
                return null;
            return new NPC(a);
        }

        public Player ToPlayer()
        {
            BasePlayer p = baseEntity.ToPlayer();
            if (p == null)
                return null;
            return Server.GetPlayer(p);
        }

        public virtual Vector3 Location {
            get {
                return baseEntity.transform.position;
            }
            set {
                bool oldsync = baseEntity.syncPosition;
                baseEntity.transform.position = value;
                baseEntity.syncPosition = true;
                baseEntity.TransformChanged();
                baseEntity.syncPosition = oldsync;
            }
        }

        public virtual string Name {
            get {
                return baseEntity.name == "player/player" ? (baseEntity as BasePlayer).displayName  : baseEntity.name;
            }
        }

        public float X {
            get {
                return baseEntity.transform.position.x;
            }
        }

        public float Y {
            get {
                return baseEntity.transform.position.y;
            }
        }

        public float Z {
            get {
                return baseEntity.transform.position.z;
            }
        }
    }
}

