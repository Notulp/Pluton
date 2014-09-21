using System;
using UnityEngine;

namespace Pluton {
	public class InvItem {

		public Item _item;

		public InvItem(string name, int amount) {
			var item = ItemManager.CreateByName(name, amount);
			if (item == null) {
				Logger.LogDebug(String.Format("[InvItem] Couldn't create item: {0}x{1}", amount, name));
				_item = null;
			}
			_item = item;
		}

		public InvItem(string name) {
			var item = ItemManager.CreateByName(name, 1);
			if (item == null) {
				Logger.LogDebug(String.Format("[InvItem] Couldn't create item: {0}x{1}", 1, name));
				_item = null;
			}
			_item = item;
		}

		public InvItem(Item item) {
			_item = item;
		}

		public bool CanStack(InvItem item){
			return _item.CanStack(item._item);
		}

		public void Drop(Transform position, Vector3 offset) {
			_item.Drop(position, offset);
		}

		public Entity Instantiate(Vector3 v3, Quaternion q) {
			return new Entity(_item.CreateWorldObject(v3, q));
		}

		public string Name {
			get {
				return _item.info.displayname;
			}
		}

		public int Quantity {
			get {
				return _item.amount;
			}
			set {
				_item.amount = value;
			}
		}

		public int Slot {
			get {
				return _item.position;
			}
			set {
				_item.position = value;
			}
		}
	}
}

