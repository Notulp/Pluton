using UnityEngine;

namespace Pluton
{
    public static class UnityEngineExtensions
    {
        public static SerializedVector3 Serialize(this Vector3 self)
        {
            return new SerializedVector3(self);
        }

        public static SerializedQuaternion Serialize(this Quaternion self)
        {
            return new SerializedQuaternion(self);
        }
    }
}

