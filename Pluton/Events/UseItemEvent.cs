using System;

namespace Pluton.Events
{
    public class UseItemEvent : CountedInstance
    {
        private InvItem _item;
        private int _amount;

        public UseItemEvent(Item item, int amountToConsume)
        {
            this._item = new InvItem(item);
            this._amount = amountToConsume;
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public int Amount
        {
            get { return this._amount; }
        }

    }
}
