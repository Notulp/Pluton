using System;

namespace Pluton.Events
{
    public class RespawnEvent
    {
        public bool GiveDefault;
        public bool ChangePos;
        public bool WakeUp;
        public UnityEngine.Vector3 SpawnPos;
        public Player Player;

        public RespawnEvent(Player p)
        {
            SpawnPos = UnityEngine.Vector3.zero;
            WakeUp = false;
            ChangePos = false;
            GiveDefault = true;
            Player = p;
        }
    }
}

