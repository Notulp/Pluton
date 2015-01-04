using System;
using UnityEngine;

namespace Pluton
{
    public class DropUpdate : MonoBehaviour
    {
        private bool landed = false;
        private bool firstRun = true;

        [NonSerialized]
        private Vector3 ground;

        [NonSerialized]
        public BaseEntity self;

        [NonSerialized]
        public BaseEntity parachute;

        [NonSerialized]
        public StorageBox box;

        public DropUpdate()
        {
        }

        private void OnLanded()
        {
            landed = true;

            self.transform.position = ground;
            self.SendNetworkUpdate(BasePlayer.NetworkQueue.Positional);

            box.inventory = new ItemContainer();
            box.inventory.capacity = 16;

            PopulateLoot();
            if (parachute != null)
                parachute.Kill();
        }

        private void Update()
        {
            if (landed) {
                if (box.inventory.itemList.Count == 0) {
                    self.Kill();
                }
                return;
            }

            Realm.PushServer();
            var pos = self.transform.position;

            if (firstRun) {
                RaycastHit hit;
                var origin = new Vector3(pos.x, pos.y - 2, pos.z);
                if (Physics.Raycast(origin, Vector3.down, out hit, 1000, Physics.AllLayers)) {
                    ground = hit.point;
                }
                firstRun = false;
            }

            float dist = pos.y - ground.y;

            if (dist < 0.05f) {
                OnLanded();
                return;
            }

            self.transform.position = UnityEngine.Vector3.Lerp(pos, ground, Time.deltaTime*0.3f);
            self.SendNetworkUpdate(BasePlayer.NetworkQueue.Positional);
            Realm.Pop();
        }

        private void AddItemRandom(float chance, string name, int minamount = 1, int maxamount = 1)
        {
            if (UnityEngine.Random.Range(0, 1) > chance) {
                return;
            }
            Item item = ItemManager.CreateByName(name, 1);
            if (item == null) {
                Debug.LogWarning("Couldn't create item " + name, this);
                return;
            }
            item.amount = UnityEngine.Random.Range(minamount, maxamount);
            item.MoveToContainer(box.inventory, -1, true);
        }

        private void PopulateLoot()
        {
            this.AddItemRandom(0.6f, "apple", 1, 3);
            this.AddItemRandom(0.1f, "furnace", 1, 1);
            this.AddItemRandom(0.05f, "ammo_rifle", 10, 200);
            this.AddItemRandom(0.05f, "ammo_pistol", 10, 200);
            this.AddItemRandom(0.05f, "ammo_shotgun", 10, 200);
            this.AddItemRandom(0.2f, "paper", 10, 200);
            this.AddItemRandom(0.15f, "chicken_cooked", 1, 2);
            this.AddItemRandom(0.2f, "can_beans", 1, 1);
            this.AddItemRandom(0.2f, "can_beans", 1, 1);
            this.AddItemRandom(0.2f, "can_tuna", 1, 1);
            this.AddItemRandom(0.02f, "rifle_bolt", 1, 1);
            this.AddItemRandom(0.02f, "shotgun_waterpipe", 1, 1);
            this.AddItemRandom(0.02f, "pistol_eoka", 1, 1);
            this.AddItemRandom(0.2f, "bandage", 1, 2);
            this.AddItemRandom(0.1f, "antiradpills", 1, 2);
            this.AddItemRandom(0.2f, "trap_bear", 1, 1);
        }
    }
}

