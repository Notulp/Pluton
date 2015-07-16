namespace Pluton.Events
{
    public class ConsumeFuelEvent : CountedInstance
    {
        private BaseOven _baseOven;
        private InvItem _item;
        private ItemModBurnable _burn;

        public ConsumeFuelEvent(BaseOven bo, Item fuel, ItemModBurnable burn)
        {
            _baseOven = bo;
            _item = new InvItem(fuel);
            _burn = burn;
        }

        public BaseOven BaseOven
        {
            get { return _baseOven; }
        }

        public InvItem Item
        {
            get { return _item; }
        }

        public ItemModBurnable Burnable
        {
            get { return _burn; }
        }

    }
}
