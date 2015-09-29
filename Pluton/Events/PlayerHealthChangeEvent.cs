namespace Pluton.Events
{
    public class PlayerHealthChangeEvent : CountedInstance
    {
        private float _oldh, _newh;
        private Player _pl;

        public PlayerHealthChangeEvent(BasePlayer p, float oldh, float newh)
        {
            _oldh = oldh;
            _newh = newh;
            _pl = new Player(p);
        }

        public Player Player
        {
            get { return _pl; }
        }

        public float OldHealth
        {
            get { return _oldh; }
        }

        public float NewHealth
        {
            get { return _newh; }
        }
    }
}
