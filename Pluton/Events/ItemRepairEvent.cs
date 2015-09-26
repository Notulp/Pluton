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
            _repairBench = repairBench;
            _player = Server.GetPlayer(msg.player);
            _item = new InvItem(repairBench.inventory.GetSlot(0));
            _msg = msg;
        }

        public RepairBench RepairBench {
            get { return _repairBench; }
        }

        public Player Player {
            get { return _player; }
        }

        public InvItem Item {
            get { return _item; }
        }

        public BaseEntity.RPCMessage RPCMessage {
            get { return _msg; }
        }
    }
}
