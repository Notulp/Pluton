/*using System;
using UnityEngine;

namespace Pluton.StructureRecorder
{
    [Serializable]
    public class StructureComponent : CountedInstance
    {
        public string Prefab;
        [Range(0, 600)]
        public float Health;
        public SerializedVector3 LocalPosition;
        public SerializedQuaternion LocalRotation;

        public StructureComponent(string prefab, SerializedVector3 v3, SerializedQuaternion q, int level)
        {
            if (level > 7)
                return;
            Prefab = prefab;
            Health = (float)(level * 85);
            LocalPosition = v3;
            LocalRotation = q;
        }

        public override string ToString()
        {
            return String.Format("{0} [pos:{1}, rot:{2}]", Prefab, LocalPosition, LocalRotation);
        }
    }
}
*/
