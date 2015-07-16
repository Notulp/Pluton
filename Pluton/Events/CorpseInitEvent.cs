namespace Pluton.Events
{
    public class CorpseInitEvent : CountedInstance
    {
        public readonly BaseCorpse Corpse;
        public readonly Entity Parent;

        public CorpseInitEvent(BaseCorpse c, BaseEntity p)
        {
            Corpse = c;
            Parent = new Entity(p);
        }
    }
}

