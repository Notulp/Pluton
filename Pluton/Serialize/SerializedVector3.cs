using System;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class SerializedVector3 : CountedInstance, ISerializable
    {
        public float x;
        public float y;
        public float z;

        public SerializedVector3(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public object Deserialize()
        {
            return new Vector3(x, y, z);
        }

        public bool Equals(Vector3 v3)
        {
            return v3.Equals(ToVector3());
        }

        public bool Equals(SerializedVector3 v3)
        {
            return v3.ToVector3().Equals(ToVector3());
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1})", x, y, z);
        }
    }
}

