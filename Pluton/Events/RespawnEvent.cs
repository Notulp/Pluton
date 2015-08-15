using UnityEngine;

namespace Pluton.Events
{
    public class RespawnEvent : CountedInstance
    {
        public bool GiveDefault;
        public bool ChangePos;
        public bool WakeUp;
        public float StartHealth = -1;
        public Vector3 SpawnPos;
        public Quaternion SpawnRot;
        public Player Player;

        public RespawnEvent(Player p, Vector3 pos, Quaternion quat)
        {
            SpawnPos = pos;
            SpawnRot = quat;
            WakeUp = false;
            ChangePos = false;
            GiveDefault = true;
            Player = p;
        }
    }
}

