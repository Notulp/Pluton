namespace Pluton.Events
{
    public class BuildingPartDemolishedEvent : CountedInstance
    {
        public readonly Player Player;
        public readonly BuildingPart BuildingPart;

        public BuildingPartDemolishedEvent(BuildingBlock bb, BasePlayer basePlayer)
        {
            BuildingPart = new BuildingPart(bb);
            Player = Server.GetPlayer(basePlayer);
        }
    }
}