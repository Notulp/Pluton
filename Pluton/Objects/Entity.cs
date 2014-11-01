using System;
using UnityEngine;

namespace Pluton
{
    public class Entity : CountedInstance
    {

        public readonly BaseEntity baseEntity;
        public readonly string Prefab;
        public readonly uint PrefabID;

        public Entity(BaseEntity ent)
        {
            baseEntity = ent;
            Prefab = baseEntity.LookupPrefabName();
            PrefabID = baseEntity.prefabID;
        }

        public void Kill()
        {
            baseEntity.Kill(ProtoBuf.EntityDestroy.Mode.Gib, 2, 0, baseEntity.transform.position);
        }

        public Vector3 Location {
            get {
                return baseEntity.transform.position;
            }
            set {
                baseEntity.transform.position.Set(value.x, value.y, value.z);
            }
        }

        public float X {
            get {
                return baseEntity.transform.position.x;
            }
            set {
                baseEntity.transform.position.Set(value, Y, Z);
            }
        }

        public float Y {
            get {
                return baseEntity.transform.position.y;
            }
            set {
                baseEntity.transform.position.Set(X, value, Z);
            }
        }

        public float Z {
            get {
                return baseEntity.transform.position.z;
            }
            set {
                baseEntity.transform.position.Set(X, Y, value);
            }
        }
    }
}

