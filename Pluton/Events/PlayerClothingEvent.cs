using System;

namespace Pluton.Events
{
    public class PlayerClothingEvent
    {
        private InvItem _item;
        private Player _player;

        public PlayerClothingEvent(PlayerInventory playerInventory, Item item)
        {
            _item = new InvItem(item);
            _player = new Player(playerInventory.GetComponent<BasePlayer>());
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
