using System;
using UnityEngine;

namespace Pluton {
	public class World {

		private static World _world;

		public void AirDrop() {
			BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", new Vector3(), new Quaternion());
			if (!(bool) ((UnityEngine.Object) entity))
				return;
			entity.Spawn(true);
		}

		public void AirDropAtPlayer(Player player) {
			BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", player.Location, player.basePlayer.transform.rotation);
			if (!(bool) ((UnityEngine.Object) entity))
				return;
			entity.Spawn(true);
		}

		public static World GetWorld() {
			return _world;
		}

		public World() {
			_world = this;
		}
	}
}

