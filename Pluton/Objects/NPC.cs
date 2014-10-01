using System;
using UnityEngine;

namespace Pluton
{
    public class NPC
    {
        public readonly BaseAnimal baseAnimal;

        public NPC(BaseAnimal animal)
        {
            baseAnimal = animal;
        }

        public void Kill()
        {
            var info = new HitInfo();
            info.damageType = Rust.DamageType.Suicide;
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
                return baseAnimal.myHealth; 
            }
        }

        public Vector3 Location {
            get {
                return baseAnimal.transform.position;
            }
            set {
                baseAnimal.transform.position.Set(value.x, value.y, value.z);
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
            set {
                baseAnimal.transform.position.Set(value, Y, Z);
            }
        }

        public float Y {
            get {
                return baseAnimal.transform.position.y;
            }
            set {
                baseAnimal.transform.position.Set(X, value, Z);
            }
        }

        public float Z {
            get {
                return baseAnimal.transform.position.z;
            }
            set {
                baseAnimal.transform.position.Set(X, Y, value);
            }
        }
    }
}

