using System;

namespace Pluton.Events
{
    public class BuildingEvent : CountedInstance
    {
        public BuildingPart BuildingPart;
        public readonly Player Builder;
        public readonly Construction Construction;
        public readonly Construction.Target Target;
        public bool NeedsValidPlacement;

        public string DestroyReason = String.Empty;
        public bool DoDestroy = false;

        public BuildingEvent(Construction construction, Construction.Target target, BuildingBlock bb, bool bNeedsValidPlacement)
        {
            this.Builder = Server.GetPlayer(target.player);
            this.BuildingPart = new BuildingPart(bb);
            this.Construction = construction;
            this.Target = target;
            this.NeedsValidPlacement = bNeedsValidPlacement;
        }

        public void Destroy(string reason = "Plugin blocks building!")
        {
            this.DoDestroy = true;
            this.DestroyReason = reason;
        }
    }
}