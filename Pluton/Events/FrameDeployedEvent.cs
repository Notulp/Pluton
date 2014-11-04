using System;
using UnityEngine;

namespace Pluton.Events
{
    public class FrameDeployedEvent : CountedInstance
    {
        public readonly Player Deployer;
        public readonly Item.Modules.Planner _planner;
        public readonly BuildingPart BuildingPart;
        public readonly InvItem invItem;

        public FrameDeployedEvent(Item.Modules.Planner planner, Item item, BasePlayer p, GameObject obj)
        {
            Deployer = Server.GetServer().Players[p.userID];
            BuildingBlock bb = obj.GetComponent<BuildingBlock>();
            BuildingPart = new BuildingPart(bb);
            invItem = new InvItem(item);
            _planner = planner;
        }
    }
}

