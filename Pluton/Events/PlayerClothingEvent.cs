namespace Pluton.Events
{
    public class PlayerClothingEvent
    {
        InvItem _item;
        Player _player;

        public PlayerClothingEvent(PlayerInventory playerInventory, Item item)
        {
            _item = new InvItem(item);
            _player = new Player(playerInventory.GetComponent<BasePlayer>());
        }

        public Player Player
        {
            get { return _player; }
        }

        public InvItem Item
        {
            get { return _item; }
        }
    }
}
