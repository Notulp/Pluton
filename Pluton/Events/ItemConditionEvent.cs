namespace Pluton.Events
{
    public class ItemConditionEvent : CountedInstance
    {
        private InvItem _item;
        private float _amount;

        public Player Player;

        public ItemConditionEvent(Item item, float amount)
        {
            _item = new InvItem(item);
            _amount = amount;
            Player = Server.GetPlayer(item.GetOwnerPlayer());
        }

        public InvItem Item {
            get { return _item; }
        }

        public float Amount {
            get { return _amount; }
        }
    }
}