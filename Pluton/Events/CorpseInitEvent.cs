namespace Pluton.Events
{
    public class CorpseInitEvent : CountedInstance
    {
        public readonly BaseCorpse Corpse;
        public readonly Entity Parent;

        public CorpseInitEvent(BaseCorpse c, BaseEntity p)
        {
            this.Corpse = c;
            this.Parent = new Entity(p);
        }
    }
}

