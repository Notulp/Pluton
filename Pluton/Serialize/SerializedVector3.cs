using System;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public struct SerializedVector3 : ISerializable
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

        public override bool Equals(object other)
        {
            if (!(other is SerializedVector3))
                return false;

            SerializedVector3 v3 = (SerializedVector3)other;

            return x.Equals(v3.x) && y.Equals(v3.y) && z.Equals(v3.z);
        }

        public override int GetHashCode()
        {
            return ToVector3().GetHashCode();
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1})", x, y, z);
        }

        public static bool operator == (SerializedVector3 lhs, SerializedVector3 rhs) {
            return Vector3.SqrMagnitude (lhs.ToVector3() - rhs.ToVector3()) < 9.999999E-11;
        }

        public static bool operator != (SerializedVector3 lhs, SerializedVector3 rhs) {
            return Vector3.SqrMagnitude (lhs.ToVector3() - rhs.ToVector3()) >= 9.999999E-11;
        }
    }
}

