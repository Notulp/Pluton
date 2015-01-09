using System;
using UnityEngine;

namespace Pluton
{
    public class DropUpdate : MonoBehaviour
    {
        public bool landed = false;

        float X;
        float Y;
        float Z;

        float groundY;

        [NonSerialized]
        public BaseEntity self;

        [NonSerialized]
        public BaseEntity parachute;

        [NonSerialized]
        public StorageBox box;

        [NonSerialized]
        Vector3 velocity;

        public Vector3 ground {
            get {
                return new Vector3(X, groundY, Z);
            }
            set {
                X = value.x;
                groundY = value.y;
                Z = value.z;
            }
        }

        public Vector3 position {
            get {
                return new Vector3(X, Y, Z);
            }
            set {
                X = value.x;
                Y = value.y;
                Z = value.z;
            }
        }

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

            parachute = null;
        }

        private void Awake()
        {
            velocity = Vector3.zero;
        }

        private void Update()
        {
            if (landed) {
                if (box.inventory.itemList.Count == 0) {
                    if (parachute != null)
                        parachute.Kill();

                    if (self!= null)
                        self.Kill();
                }
                return;
            }

            Realm.PushServer();
            position = self.transform.position;

            float dist = Y - groundY;

            if (dist < 0.05f) {
                OnLanded();
                return;
            }

            self.transform.position = Vector3.SmoothDamp(self.transform.position, self.transform.position - new Vector3(0f, 0.5f, 0f), ref velocity, 0.3f); 
            self.SendNetworkUpdate(BasePlayer.NetworkQueue.Positional);
            Realm.Pop();
        }

        private void AddItemRandom(float chance, string name, int minamount = 1, int maxamount = 1)
        {
            if (UnityEngine.Random.Range(0f, 100f) > chance) {
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
            // chance (%), itemname, min amount, max amount 
            AddItemRandom(60f, "apple", 1, 3);
            AddItemRandom(10f, "furnace", 1, 1);
            AddItemRandom(5f, "ammo_rifle", 10, 200);
            AddItemRandom(5f, "ammo_pistol", 10, 200);
            AddItemRandom(5f, "ammo_shotgun", 10, 200);
            AddItemRandom(20f, "paper", 10, 200);
            AddItemRandom(15f, "chicken_cooked", 1, 2);
            AddItemRandom(2f, "rifle_bolt", 1, 1);
            AddItemRandom(2f, "shotgun_waterpipe", 1, 1);
            AddItemRandom(2f, "pistol_eoka", 1, 1);
            AddItemRandom(20f, "bandage", 1, 2);
            AddItemRandom(10f, "antiradpills", 1, 2);
        }
    }
}

