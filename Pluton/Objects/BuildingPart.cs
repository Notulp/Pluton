using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class BuildingPart : Entity
    {
        [NonSerialized]
        private BuildingBlock _buildingBlock;

        SerializedVector3 position;

        [OnSerializing]
        void OnSerializing(StreamingContext context)
        {
            if (position != null)
                return;
            position = buildingBlock.transform.position.Serialize();
        }

        public BuildingPart(BuildingBlock bb) : base(bb)
        {
            _buildingBlock = bb;
        }

        public Construction.Socket FindSocket(string name)
        {
            return buildingBlock.FindSocket(name);
        }

        public void Destroy()
        {
            buildingBlock.Kill(ProtoBuf.EntityDestroy.Mode.Gib, 2, 0, buildingBlock.transform.position);
        }

        public override bool IsBuildingPart()
        {
            return true;
        }

        public void Rotate()
        {
            Construction.Grade grade = buildingBlock.blockDefinition.grades[buildingBlock.grade];
            if (!grade.canRotate) {
                return;
            }
            buildingBlock.transform.localRotation *= Quaternion.Euler(grade.rotationAmount);

            NetworkData networkData = new NetworkData();
            networkData.WriteUInt32(StringPool.Get("updateconditionalmodels"));
            networkData.WriteUInt64(0);
            buildingBlock.net.Broadcast(MSG.RPC_MESSAGE, networkData.ToBytes(), Network.SendMethod.Reliable);

            buildingBlock.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
        }

        public BuildingBlock buildingBlock {
            get {
                if (_buildingBlock == null) {
                    Vector3 v3pos = position.ToVector3();
                    _buildingBlock = (from bb in UnityEngine.Object.FindObjectsOfType<BuildingBlock>()
                              where this.Prefab == bb.LookupPrefabName() &&
                                  v3pos == bb.transform.position
                              select bb).FirstOrDefault();
                }
                return _buildingBlock;
            }
            private set {
                _buildingBlock = value;
            }
        }

        public int Grade {
            get {
                return buildingBlock.grade;
            }
            set {
                buildingBlock.SetGrade(value);
                buildingBlock.SetHealthToMax();
                buildingBlock.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            }
        }

        public float Health {
            get {
                return buildingBlock.health;
            }
            set {
                buildingBlock.health = value;
            }
        }
    }
}

