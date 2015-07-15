namespace Pluton.Events
{
    public class RespawnEvent : CountedInstance
    {
        public bool GiveDefault;
        public bool ChangePos;
        public bool WakeUp;
        public float StartHealth = -1;
        public UnityEngine.Vector3 SpawnPos;
        public Player Player;

        public RespawnEvent(Player p)
        {
            this.SpawnPos = UnityEngine.Vector3.zero;
            this.WakeUp = false;
            this.ChangePos = false;
            this.GiveDefault = true;
            this.Player = p;
        }
    }
}

