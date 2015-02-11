using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class BuildingEvent : CountedInstance
    {
        public BuildingPart BuildingPart;
        public readonly Player Builder;
        public readonly Construction.Common Common;
        public readonly Construction.Target Target;
        public bool NeedsValidPlacement;

        public string DestroyReason = String.Empty;
        public bool DoDestroy = false;

        public BuildingEvent(Construction.Common common, Construction.Target target, BuildingBlock bb, bool bNeedsValidPlacement)
        {
            Builder = Server.GetPlayer(target.player);
            BuildingPart = new BuildingPart(bb);
            Common = common;
            Target = target;
            NeedsValidPlacement = bNeedsValidPlacement;
        }

        public void Destroy(string reason = "Plugin blocks building!")
        {
            DoDestroy = true;
            DestroyReason = reason;
        }
    }
}