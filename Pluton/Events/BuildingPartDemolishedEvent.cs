namespace Pluton.Events
{
    public class BuildingPartDemolishedEvent : CountedInstance
    {
        private Player player;
        private BuildingPart buildingPart;

        public BuildingPartDemolishedEvent(BuildingBlock bb, BasePlayer basePlayer)
        {
            this.buildingPart = new BuildingPart(bb);
            this.player = new Player(basePlayer);
        }

        public Player Player
        {
            get { return this.player; }
        }

        public BuildingPart BuildingPart
        {
            get { return this.buildingPart; }
        }
    }
}