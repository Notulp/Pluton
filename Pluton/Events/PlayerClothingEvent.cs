namespace Pluton.Events
{
    public class PlayerClothingEvent : CountedInstance
    {
        private InvItem _item;
        private Player _player;

        public PlayerClothingEvent(PlayerInventory playerInventory, Item item)
        {
            this._item = new InvItem(item);
            this._player = Server.GetPlayer(playerInventory.GetComponent<BasePlayer>());
        }

        public Player Player
        {
            get { return this._player; }
        }

        public InvItem Item
        {
            get { return this._item; }
        }
    }
}
