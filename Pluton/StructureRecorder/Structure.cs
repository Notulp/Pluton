/*using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Pluton.StructureRecorder
{
    [Serializable]
    public class Structure : CountedInstance
    {
        public string Name;
        public Origo origo;

        public Dictionary<string, StructureComponent> Components;

        public Structure(string name)
        {
            Name = name;
            Components = new Dictionary<string, StructureComponent>();
        }

        public void AddComponent(BuildingPart bp)
        {
            if (origo == null)
                origo = new Origo(new SerializedVector3(bp.Location), new SerializedQuaternion(bp.buildingBlock.transform.rotation));
            SerializedVector3 v3 = new SerializedVector3(bp.Location - origo.Position.ToVector3());
            SerializedQuaternion q = new SerializedQuaternion(bp.buildingBlock.transform.rotation);
            StructureComponent component = new StructureComponent(bp.buildingBlock.LookupPrefabName(), v3, q, (int)Math.Floor((double)(bp.Health / 85)));

            if (component == null) {
                Logger.LogDebug("[Structure] Component is null!");
                return;
            }

            if (!Components.ContainsKey(component.ToString()))
                Components.Add(component.ToString(), component);
            else
                Components[component.ToString()] = component;
        }

        public void Build(Vector3 spawnAt)
        {
            foreach (StructureComponent component in Components.Values) {
                Vector3 v3 = (component.LocalPosition.ToVector3() + spawnAt);
                BaseEntity ent = World.GetInstance().SpawnMapEntity(component.Prefab, v3, component.LocalRotation.ToQuaternion());
                BuildingBlock bb = ent.GetComponent<BuildingBlock>();
                bb.blockHealth = component.Health;
                bb.SendNetworkUpdateImmediate();
            }
        }

        public void Export()
        {
            string path = Path.Combine(Util.GetStructuresFolder(), Name + ".sps");
            using (FileStream stream = new FileStream(path, FileMode.Create)) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
        }

        public void RemoveComponent(BuildingPart bp)
        {
            SerializedVector3 v3 = new SerializedVector3(bp.Location - origo.Position.ToVector3());
            SerializedQuaternion q = new SerializedQuaternion(bp.buildingBlock.transform.rotation);
            StructureComponent component = new StructureComponent(bp.buildingBlock.LookupPrefabName(), v3, q, (int)Math.Floor((double)(bp.Health / 85)));
            if (Components.ContainsKey(component.ToString()))
                Components.Remove(component.ToString());
        }

        public override string ToString()
        {
            return String.Format("Structure ({0}, {1})", Name, Components.Count);
        }
    }
}

*/