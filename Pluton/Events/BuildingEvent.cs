using System;
using System.Collections.Generic;

namespace Pluton.Events {
	public class BuildingEvent {

		public List<Construction.Socket> Sockets;
		public readonly BuildingBlock _block;
		public List<ItemAmount> NeededItems;
		public BuildingPart BuildingPart;
		public readonly Player Builder;
		public string BlockFullName;
		public float Proficiency;
		public string BlockName;

		public BuildingEvent(BuildingPart bp, Player player, float prof) {
			NeededItems = bp.NeededItems;
			_block = bp.buildingBlock;
			Proficiency = prof;
			BuildingPart = bp;
			Builder = player;

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