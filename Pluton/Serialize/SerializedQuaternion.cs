using System;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public struct SerializedQuaternion : ISerializable
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

        public object Deserialize()
        {
            return new Quaternion(x, y, z, w);
        }

        public override bool Equals(object other)
        {
            if (!(other is SerializedQuaternion)) {
                return false;
            }
            SerializedQuaternion q = (SerializedQuaternion)other;
            return x.Equals (q.x) && y.Equals (q.y) && z.Equals (q.z) && w.Equals (q.w);
        }

        public override int GetHashCode()
        {
            return ToQuaternion().GetHashCode();
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public override string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[] { x, y, z, w });
        }

        public static bool operator == (SerializedQuaternion lhs, SerializedQuaternion rhs) {
            return Quaternion.Dot (lhs.ToQuaternion(), rhs.ToQuaternion()) > 0.999999;
        }

        public static bool operator != (SerializedQuaternion lhs, SerializedQuaternion rhs) {
            return Quaternion.Dot (lhs.ToQuaternion(), rhs.ToQuaternion()) <= 0.999999;
        }
    }
}

