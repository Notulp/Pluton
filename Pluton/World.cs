using System;
using UnityEngine;

namespace Pluton
{
    public class World
    {

        private static World instance;

        public void AirDrop()
        {
            BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", Vector3.zero, Quaternion.identity);
            if (entity != null)
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
            return ground + 2; // increase by 2, as prefabs/players will die without it.
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

