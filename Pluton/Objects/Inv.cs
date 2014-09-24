using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton {
	public class Inv {

		private readonly PlayerInventory _inv;
		public readonly Player owner;

		public Inv(PlayerInventory inv) {
			_inv = inv;
			owner = new Player(inv.GetComponent<BasePlayer>());
		}

		public bool Add(InvItem item) {
			return _inv.GiveItem(item._item);
		}

		public bool Add(InvItem item, ItemContainer con) {
			return _inv.GiveItem(item._item, con);
		}

		public bool Add(int itemID) {
			return Add(itemID, 1);
		}

		public bool Add(int itemID, int amount) {
			return _inv.GiveItem(itemID, amount, true);
		}

		public ItemContainer InnerBelt() {
			return _inv.containerBelt;
		}

		public ItemContainer InnerMain() {
			return _inv.containerMain;
		}

		public ItemContainer InnerWear() {
			return _inv.containerWear;
		}

		public List<InvItem> AllItems() {
			return (from item in _inv.AllItems()
				select new InvItem(item)).ToList();
		}

		public List<InvItem> BeltItems() {
			return (from item in _inv.containerBelt.itemList
				select new InvItem(item)).ToList();
		}

		public List<InvItem> MainItems() {
			return (from item in _inv.containerMain.itemList
				select new InvItem(item)).ToList();
		}

		public List<InvItem> WearItems() {
			return (from item in _inv.containerWear.itemList
				select new InvItem(item)).ToList();
		}
	}
}

