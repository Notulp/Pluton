namespace Pluton.Events
{
    public class ItemConditionEvent : CountedInstance
    {
        private InvItem _item;
        private float _amount;

        public ItemConditionEvent(Item item, float amount)
        {
            this._item = new InvItem(item);
            this._amount = amount;
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public float Amount
        {
            get { return this._amount; }
        }
    }
}