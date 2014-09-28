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
			ItemContainer con;
			if (item.containerPref == InvItem.ContainerPreference.Belt)
				con = InnerBelt;
			else if (item.containerPref == InvItem.ContainerPreference.Wear)
				con = InnerWear;
			else
				con = InnerMain;

			bool flag = _inv.GiveItem(item._item, con);
			if (!flag) {
				flag = _inv.GiveItem(item._item);
			}

			return flag;
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

		public ItemContainer InnerBelt {
			get {
				return _inv.containerBelt;
			}
		}

		public ItemContainer InnerMain {
			get {
				return _inv.containerMain;
			}
		}

		public ItemContainer InnerWear {
			get {
				return _inv.containerWear;
			}
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

