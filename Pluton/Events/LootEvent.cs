namespace Pluton.Events
{
    public class LootEvent : CountedInstance
    {
        public bool Cancel = false;
        public string cancelReason = "A plugin stops you from looting that!";
        public readonly Player Looter;
        public readonly PlayerLoot pLoot;

        public LootEvent(PlayerLoot pl, Player looter)
        {
            this.Looter = looter;
            this.pLoot = pl;
        }

        public void Stop(string reason = "A plugin stops you from looting that!")
        {
            this.cancelReason = reason;
            this.Cancel = true;
        }
    }
}

