using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class BuildingPart : CountedInstance
    {
        [NonSerialized]
        private BuildingBlock _buildingBlock;

        public readonly string Prefab;
        public readonly uint PrefabID;

        SerializedVector3 position;

        [OnSerializing]
        void OnSerializing(StreamingContext context)
        {
            if (position != null)
                return;
            position = buildingBlock.transform.position.Serialize();
        }

        public BuildingPart(BuildingBlock bb)
        {
            _buildingBlock = bb;
            Prefab = bb.LookupPrefabName();
            PrefabID = bb.prefabID;
        }

        public void CancelDemolish()
        {
            buildingBlock.beingDemolished = false;
            buildingBlock.Invoke("HealthCreepCheck", 0f);
        }

        public Construction.Socket FindSocket(string name)
        {
            return buildingBlock.FindSocket(name);
        }

        public void Destroy()
        {
            buildingBlock.Kill(ProtoBuf.EntityDestroy.Mode.Gib, 2, 0, buildingBlock.transform.position);
        }

        public void Rotate()
        {
            Construction.Grade grade = buildingBlock.blockDefinition.grades[buildingBlock.grade];
            if (!grade.canRotate) {
                return;
            }
            buildingBlock.transform.localRotation *= Quaternion.Euler(grade.rotationAmount);

            ProtoBuf.RPCMessage rPCMessage = new ProtoBuf.RPCMessage();
            rPCMessage.funcName = StringPool.Get("updateconditionalmodels");
            rPCMessage.data = null;
            buildingBlock.net.Broadcast(MSG.RPC_MESSAGE, rPCMessage.ToProtoBytes(), Network.SendMethod.Reliable);

            buildingBlock.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
        }

        public void StartDemolish()
        {
            buildingBlock.beingDemolished = true;
            buildingBlock.Invoke("HealthCreepCheck", 0f);
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

        public Vector3 Location {
            get {
                return buildingBlock.transform.position;
            }
        }

        public float X {
            get {
                return buildingBlock.transform.position.x;
            }
        }

        public float Y {
            get {
                return buildingBlock.transform.position.y;
            }
        }

        public float Z {
            get {
                return buildingBlock.transform.position.z;
            }
        }
    }
}

