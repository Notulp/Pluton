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

        public BuildingPart ToBuildingPart()
        {
            BuildingBlock b = baseEntity.GetComponent<BuildingBlock>();
            if (b == null)
                return null;
            return new BuildingPart(b);
        }

        public NPC ToNPC()
        {
            BaseAnimal a = baseEntity.GetComponent<BaseAnimal>();
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

        public Vector3 Location {
            get {
                return baseEntity.transform.position;
            }
        }

        public string Name {
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

