using System;
using UnityEngine;

namespace Pluton
{
    public class World
    {

        private static World instance;

        public void AirDrop()
        {
            // TODO: check to see if there is water or a building
            System.Random rand = new System.Random();
            Vector3 v3 = new Vector3(rand.Next(-1600, 1600), 0f, rand.Next(-1600, 1600));
            BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", v3, new Quaternion());
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
        
        public void Spawn(string prefab, float x, float z) {
            Spawn(prefab, x, GetGround(x, z), z);    
        }
        
        public void Spawn(string prefab, float x, float y, float z) {
                GameManager.CreateEntity("events/" + prefab, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion()).Spawn(true);
        }

        public static World GetWorld()
        {
            if (instance == null)
                instance = new World();
            return instance;
        }
    }
}

