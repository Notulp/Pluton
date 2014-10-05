using System;

namespace Pluton.Events
{
    public class RespawnEvent
    {
        public bool GiveDefault;
        public bool ChangePos;
        public UnityEngine.Vector3 SpawnPos;
        public Player Player;

        public RespawnEvent(Player p)
        {
            SpawnPos = UnityEngine.Vector3.zero;
            ChangePos = false;
            GiveDefault = true;
            Player = p;
        }
    }
}

