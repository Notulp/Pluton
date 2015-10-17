namespace Pluton.Events
{
    public class DoorUseEvent : CountedInstance
    {
        public bool Allow = true; // allow the player to open ( or try to open with a lock ) the door ?
        public bool Open; // is the player opening or closing the door ?
        public bool IgnoreLock = false;
        public Player Player;
        public Entity Door;

        public string DenyReason = "";

        public DoorUseEvent(Entity door, Player player, bool open)
        {
            Door = door;
            Open = open;
            Player = player;
        }

        public void Deny(string reason = "")
        {
            Allow = false;
            DenyReason = reason;
        }
    }
}

