using System;
using System.Collections.Generic;

namespace Pluton.Events {
	public class BuildingEvent {

		public List<Construction.Socket> Sockets;
		public readonly BuildingBlock _block;
		public List<ItemAmount> NeededItems;
		public readonly Player Builder;
		public string BlockFullName;
		public float Proficiency;
		public string BlockName;

		public BuildingEvent(BuildingBlock bb, Player player, float prof) {
			NeededItems = bb.neededItems;
			Proficiency = prof;
			Builder = player;
			_block = bb;

			if (bb.blockDefinition != null) {
				BlockName = bb.blockDefinition.name;
				Sockets = bb.blockDefinition.sockets;
				BlockFullName = bb.blockDefinition.fullname;
			}
		}
	}
}

/*
 * possible properties:
 * player.basePlayer.svActiveItem as (constructor item)
 * 
 */