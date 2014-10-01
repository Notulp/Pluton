using System;
using UnityEngine;

namespace Pluton
{
    public class World
    {

        private static World instance;

        public void AirDrop()
        {
            BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", new Vector3(), new Quaternion());
            if (!(bool)((UnityEngine.Object)entity))
                return;
            entity.Spawn(true);
        }

        public void AirDropAtPlayer(Player player)
        {
            BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", player.Location, player.basePlayer.transform.rotation);
            if (!(bool)((UnityEngine.Object)entity))
                return;
            entity.Spawn(true);
        }

        public float GetGround(float x, float z)
        {
            RaycastHit hit;
            var origin = new Vector3(x, 5000, z);
            float ground = 0f;
            if (Physics.Raycast(origin, Vector3.down, out hit, 8000, Physics.AllLayers)) {
                ground = hit.point.y;
            }
            return ground;
        }

        public float GetGround(Vector3 v3)
        {
            return GetGround(v3.x, v3.z);
        }

        public static World GetWorld()
        {
            if (instance == null)
                instance = new World();
            return instance;
        }
    }
}

