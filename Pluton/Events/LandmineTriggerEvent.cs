namespace Pluton.Events
{
    public class LandmineTriggerEvent
    {
        private Landmine _landmine;
        private Player _player;
        private bool _explode = true;

        public LandmineTriggerEvent(Landmine landmine, BasePlayer player)
        {
            _landmine = landmine;
            _player = Server.GetPlayer(player);
        }

        public Landmine Landmine
        {
            get { return _landmine; }
        }

        public Player Player
        {
            get { return _player; }
        }

        public bool Explode
        {
            get { return _explode; }
        }

        public void CancelExplosion()
        {
            _explode = false;
        }
    }
}
