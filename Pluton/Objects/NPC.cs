using System;
using UnityEngine;

namespace Pluton
{
    public class NPC : CountedInstance
    {
        public readonly BaseAnimal baseAnimal;

        public NPC(BaseAnimal animal)
        {
            baseAnimal = animal;
        }

        public void Kill()
        {
            var info = new HitInfo();
            info.AddDamage(Rust.DamageType.Suicide, 100f);
            info.Initiator = baseAnimal as BaseEntity;
            baseAnimal.Die(info);
        }

        public uint ID {
            get {
                return baseAnimal.net.ID; 
            }
        }

        public float Health {
            get {
                return baseAnimal.health;
            }
            set {
                baseAnimal.health = value;
            }

        }

        public Vector3 Location {
            get {
                return baseAnimal.transform.position;
            }
        }

        public string Name {
            get {
                return baseAnimal.name;
            }
        }

        public string Prefab {
            get {
                return baseAnimal.modelPrefab;
            }
        }

        public float X {
            get {
                return baseAnimal.transform.position.x;
            }
        }

        public float Y {
            get {
                return baseAnimal.transform.position.y;
            }
        }

        public float Z {
            get {
                return baseAnimal.transform.position.z;
            }
        }
    }
}

