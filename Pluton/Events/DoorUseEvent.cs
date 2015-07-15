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
            this.Door = door;
            this.Open = open;
            this.Player = player;
        }

        public void Deny(string reason = "")
        {
            this.Open = !Open;
            this.DenyReason = reason;
        }
    }
}

