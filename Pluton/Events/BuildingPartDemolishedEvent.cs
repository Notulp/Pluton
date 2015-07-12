using System;

namespace Pluton.Events
{
    public class BuildingPartDemolishedEvent : CountedInstance
    {
        private Player player;
        private BuildingPart buildingPart;

        public BuildingPartDemolishedEvent(BuildingBlock bb, BasePlayer basePlayer)
        {
            buildingPart = new BuildingPart(bb);
            player = new Player(basePlayer);
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