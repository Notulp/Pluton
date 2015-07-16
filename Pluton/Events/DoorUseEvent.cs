namespace Pluton.Events
{
    public class DoorUseEvent : CountedInstance
    {
        public bool Open;
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
            Open = !Open;
            DenyReason = reason;
        }
    }
}

