using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pluton
{
    public class BuildingPart : CountedInstance
    {

        public readonly BuildingBlock buildingBlock;

        public BuildingPart(BuildingBlock bb)
        {
            buildingBlock = bb;
        }

        public Construction.Socket FindSocket(string name)
        {
            return buildingBlock.FindSocket(name);
        }

        public void ChangeOwner(Player player)
        {
            if (player == null)
                return;

            OwnerID = player.GameID;
            OwnerName = player.Name;
        }

        public void Destroy()
        {
            buildingBlock.Kill(ProtoBuf.EntityDestroy.Mode.Gib, 2, 0, buildingBlock.transform.position);
        }

        public float Health {
            get {
                return buildingBlock.health;
            }
            set {
                buildingBlock.health = value;
            }
        }

        public bool IsFrame {
            get {
                return buildingBlock.IsFrame();
            }
        }

        public int ItemIDBase {
            get {
                return buildingBlock.ItemIDBase;
            }
        }

        public Vector3 Location {
            get {
                return buildingBlock.transform.position;
            }
            set {
                buildingBlock.transform.position.Set(value.x, value.y, value.z);
            }
        }

        public ulong OwnerID {
            get {
                return buildingBlock.deployerUserID;
            }
            set {
                buildingBlock.deployerUserID = value;
            }
        }

        public string OwnerName {
            get {
                return buildingBlock.deployerUserName;
            }
            set {
                buildingBlock.deployerUserName = value;
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

