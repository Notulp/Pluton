namespace Pluton.Events
{
    public class ItemRepairEvent : CountedInstance
    {
        private RepairBench _repairBench;
        private Player _player;
        private InvItem _item;
        private BaseEntity.RPCMessage _msg;
        
        public ItemRepairEvent(RepairBench repairBench, BaseEntity.RPCMessage msg)
        {
            this._repairBench = repairBench;
            this._player = Server.GetPlayer(msg.player);
            this._item = new InvItem(repairBench.inventory.GetSlot(0));
            this._msg = msg;
        }

        public RepairBench RepairBench
        {
            get { return this._repairBench; }
        }

        public Player Player
        {
            get { return this._player; }
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public BaseEntity.RPCMessage RPCMessage
        {
            get { return this._msg; }
        }
    }
}
