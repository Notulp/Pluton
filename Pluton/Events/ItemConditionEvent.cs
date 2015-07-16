namespace Pluton.Events
{
    public class ItemConditionEvent : CountedInstance
    {
        private InvItem _item;
        private float _amount;

        public ItemConditionEvent(Item item, float amount)
        {
            _item = new InvItem(item);
            _amount = amount;
        }

        public InvItem Item
        {
            get { return _item; }
        }

        public float Amount
        {
            get { return _amount; }
        }
    }
}