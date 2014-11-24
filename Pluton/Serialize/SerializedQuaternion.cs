using System;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class SerializedQuaternion : CountedInstance
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializedQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public bool Equals(Quaternion q)
        {
            return q.Equals(ToQuaternion());
        }

        public bool Equals(SerializedQuaternion q)
        {
            return q.ToQuaternion().Equals(ToQuaternion());
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public override string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[] { x, y, z, w });
        }
    }
}

