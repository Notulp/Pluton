using System;

namespace Pluton.StructureRecorder
{
    [Serializable]
    public class Origo : CountedInstance
    {
        public SerializedVector3 Position;
        public SerializedQuaternion Rotation;

        public Origo(SerializedVector3 v3, SerializedQuaternion q)
        {
            Position = v3;
            Rotation = q;
        }
    }
}

