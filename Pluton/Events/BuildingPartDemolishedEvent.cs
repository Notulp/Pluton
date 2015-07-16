namespace Pluton.Events
{
    public class BuildingPartDemolishedEvent : CountedInstance
    {
        private Player player;
        private BuildingPart buildingPart;

        public BuildingPartDemolishedEvent(BuildingBlock bb, BasePlayer basePlayer)
        {
            buildingPart = new BuildingPart(bb);
            player = Server.GetPlayer(basePlayer);
        }

        public Player Player
        {
            get { return player; }
        }

        public BuildingPart BuildingPart
        {
            get { return buildingPart; }
        }
    }
}