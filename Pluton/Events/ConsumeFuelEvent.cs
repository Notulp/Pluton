using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace Pluton.Events
{
    public class ConsumeFuelEvent
    {
        private BaseOven _baseOven;
        private InvItem _item;
        private ItemModBurnable _burn;

        public ConsumeFuelEvent(BaseOven bo, Item fuel, ItemModBurnable burn)
        {
            this._baseOven = bo;
            this._item = new InvItem(fuel);
            this._burn = burn;
        }

        public BaseOven BaseOven
        {
            get { return this._baseOven; }
        }

        public InvItem Item
        {
            get { return this._item; }
        }

        public ItemModBurnable Burnable
        {
            get { return this._burn; }
        }

    }
}
