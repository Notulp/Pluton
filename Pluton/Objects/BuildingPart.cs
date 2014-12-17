using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluton
{
    public class BuildingPart : CountedInstance
    {

        public readonly BuildingBlock buildingBlock;
        public readonly string Prefab;
        public readonly uint PrefabID;

        public BuildingPart(BuildingBlock bb)
        {
            buildingBlock = bb;
            Prefab = buildingBlock.LookupPrefabName();
            PrefabID = buildingBlock.prefabID;
        }

        public Construction.Socket FindSocket(string name)
        {
            return buildingBlock.FindSocket(name);
        }

        public void Destroy()
        {
            buildingBlock.Kill(ProtoBuf.EntityDestroy.Mode.Gib, 2, 0, buildingBlock.transform.position);
        }

        /*public float Health {
            get {
                return buildingBlock.blockHealth;
            }
            set {
                buildingBlock.blockHealth = value;
            }
        }*/

        public Vector3 Location {
            get {
                return buildingBlock.transform.position;
            }
            set {
                buildingBlock.transform.position.Set(value.x, value.y, value.z);
            }
        }

        public float X {
            get {
                return buildingBlock.transform.position.x;
            }
            set {
                buildingBlock.transform.position.Set(value, Y, Z);
            }
        }

        public float Y {
            get {
                return buildingBlock.transform.position.y;
            }
            set {
                buildingBlock.transform.position.Set(X, value, Z);
            }
        }

        public float Z {
            get {
                return buildingBlock.transform.position.z;
            }
            set {
                buildingBlock.transform.position.Set(X, Y, value);
            }
        }
    }
}

