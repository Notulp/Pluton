namespace Pluton.Events
{
    public class HammerEvent : CountedInstance
    {
        public readonly HitInfo _info;
        public readonly string HitBone;
        public readonly BasePlayer basePlayer;

        public HammerEvent(HitInfo info, BasePlayer player)
        {
            _info = info;
            basePlayer = player;
            string bonename = StringPool.Get(info.HitBone);
            HitBone = bonename == "" ? "unknown" : bonename;
        }

        public Player Player {
            get { return Server.GetPlayer(basePlayer); }
        }

        public Entity Victim {
            get {
                BaseEntity ent = _info.HitEntity;
                BasePlayer p = ent.GetComponent<BasePlayer>();
                if (p != null)
                    return Server.GetPlayer(p);
                BaseNPC n = ent.GetComponent<BaseNPC>();
                if (n != null)
                    return new NPC(n);
                return new Entity(ent);
            }
        }
    }
}
