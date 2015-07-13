namespace Pluton.Events
{
    public class LootEvent : CountedInstance
    {
        public bool Cancel;
        public string cancelReason = "A plugin stops you from looting that!";
        public readonly Player Looter;
        public readonly PlayerLoot pLoot;

        public LootEvent(PlayerLoot pl, Player looter)
        {
            Looter = looter;
            pLoot = pl;
        }

        public void Stop(string reason = "A plugin stops you from looting that!")
        {
            cancelReason = reason;
            Cancel = true;
        }
    }
}

