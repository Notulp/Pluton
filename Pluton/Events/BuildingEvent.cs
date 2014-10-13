using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class BuildingEvent
    {
        public List<Construction.Socket> Sockets;
        public readonly BuildingBlock _block;
        public BuildingPart BuildingPart;
        public readonly Player Builder;
        public string BlockFullName;
        public float Proficiency;
        public string BlockName;
        public InvItem Tool;

        public BuildingEvent(BuildingPart bp, HitInfo info)
        {

            BasePlayer player = info.Initiator as BasePlayer;
            var p = new Player(player);
            Tool = new InvItem(info.Weapon);
            _block = bp.buildingBlock;
            Proficiency = info.resourceGatherProficiency;
            BuildingPart = bp;
            Builder = p;

            if (_block.blockDefinition != null) {
                BlockName = _block.blockDefinition.name;
                Sockets = _block.blockDefinition.sockets;
                BlockFullName = _block.blockDefinition.fullname;
            }
        }
    }
}

/*
 * possible properties:
 * player.basePlayer.svActiveItem as (constructor item)
 * 
 */