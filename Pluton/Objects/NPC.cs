using System;
using UnityEngine;

namespace Pluton
{
    public class NPC : CountedInstance
    {
        public readonly BaseNPC baseNPC;

        public NPC(BaseNPC npc)
        {
            baseNPC = npc;
        }

        public void Kill()
        {
            var info = new HitInfo();
            info.AddDamage(Rust.DamageType.Suicide, 100f);
            info.Initiator = baseNPC as BaseEntity;
            baseNPC.Die(info);
        }

        public uint ID {
            get {
                return baseNPC.net.ID; 
            }
        }

        public float Health {
            get {
                return baseNPC.health;
            }
            set {
                baseNPC.health = value;
            }

        }

        public Vector3 Location {
            get {
                return baseNPC.transform.position;
            }
        }

        public string Name {
            get {
                return baseNPC.name;
            }
        }

        public string Prefab {
            get {
                return baseNPC.modelPrefab;
            }
        }

        public float X {
            get {
                return baseNPC.transform.position.x;
            }
        }

        public float Y {
            get {
                return baseNPC.transform.position.y;
            }
        }

        public float Z {
            get {
                return baseNPC.transform.position.z;
            }
        }
    }
}

