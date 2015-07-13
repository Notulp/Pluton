namespace Pluton.Events
{
    public class ItemUsedEvent : CountedInstance
    {
        InvItem _item;
        int _amount;

        public ItemUsedEvent(Item item, int amountToConsume)
        {
            _item = new InvItem(item);
            _amount = amountToConsume;
        }

        public InvItem Item
        {
            get { return _item; }
        }

        public int Amount
        {
            get { return _amount; }
        }

    }
}
