using System;
using System.Linq;
using UnityEngine;

namespace Pluton
{
    public class InvItem : CountedInstance
    {
        public readonly Item _item;

        public ContainerPreference containerPref;

        public enum ContainerPreference
        {
            Belt,
            Main,
            Wear
        }

        public InvItem(string name, int amount)
        {
            var item = ItemManager.CreateByItemID(GetItemID(name), amount);
            if (item == null) {
                Logger.LogDebug(String.Format("[InvItem] Couldn't create item: {0}x{1}", amount, name));
                _item = null;
            } else {
                _item = item;
                containerPref = GetContainerPreference(Category);
            }
        }

        public InvItem(string name)
        {
            var item = ItemManager.CreateByItemID(GetItemID(name), 1);
            if (item == null) {
                Logger.LogDebug(String.Format("[InvItem] Couldn't create item: {0}x{1}", 1, name));
                _item = null;
            } else {
                _item = item;
                containerPref = GetContainerPreference(Category);
            }
        }

        public InvItem(Item item)
        {
            _item = item;
            containerPref = GetContainerPreference(Category);
        }

        public int Amount {
            get {
                return _item.amount;
            }
            set {
                _item.amount = value;
            }
        }

        public bool CanStack(InvItem item)
        {
            return _item.CanStack(item._item);
        }

        public string Category {
            get {
                return _item.info.category.ToString();
            }
        }

        public float Condition {
            get {
                return _item.condition;
            }
            set {
                _item.condition = value;
            }
        }

        public void Drop(Vector3 position, Vector3 offset)
        {
            _item.Drop(position, offset);
        }

        public float Fuel {
            get {
                return _item.fuel;
            }
            set {
                _item.fuel = value;
            }
        }

        public Entity Instantiate(Vector3 v3)
        {
            return new Entity(_item.CreateWorldObject(v3, Quaternion.identity));
        }

        public Entity Instantiate(Vector3 v3, Quaternion q)
        {
            return new Entity(_item.CreateWorldObject(v3, q));
        }

        public bool IsBlueprint {
            get {
                return _item.HasFlag(Item.Flag.Blueprint);
            }
            set {
                _item.SetFlag(Item.Flag.Blueprint, value);
            }
        }

        public int ItemID {
            get {
                return _item.info.itemid;
            }
        }

        public string Name {
            get {
                return _item.info.displayName.english;
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

        public static int GetItemID(string itemName)
        {
            return (from item in ItemManager.itemList
                             where item.displayName.english == itemName ||
                                 item.displayName.translated == itemName ||
                                 item.shortname == itemName
                             select item.itemid).FirstOrDefault<int>();
        }

        public static ContainerPreference GetContainerPreference(string category)
        {
            if ("WeaponConstructionTool".Contains(category))
                return ContainerPreference.Belt;
            if (category == "Attire")
                return ContainerPreference.Wear;
            return ContainerPreference.Main;
        }
    }
}

