
namespace Pluton
{
    public class NPC : Entity
    {
        public readonly BaseNPC baseNPC;

        public NPC(BaseNPC npc) : base(npc)
        {
            baseNPC = npc;
        }

        public override void Kill()
        {
            var info = new HitInfo();
            info.damageTypes.Add(Rust.DamageType.Suicide, 100f);
            info.Initiator = baseNPC as BaseEntity;
            baseNPC.Die(info);
        }

        public override bool IsNPC()
        {
            return true;
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
    }
}

