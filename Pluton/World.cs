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

        public void SpawnMapEntity(string evt, float x, float z) {
            SpawnMapEntity(evt, x, GetGround(x, z), z);    
        }

        public void SpawnMapEntity(string evt, Vector3 loc) {
            SpawnMapEntity(evt, loc.x, loc.x, loc.z);    
        }
        
        public void SpawnAnimal(string name, float x, float z) {
            SpawnAnimal(name, x, GetGround(x, z), z);    
        }

        public void SpawnAnimal(string name, Vector3 loc) {
            SpawnAnimal(name, loc.x, loc.x, loc.z);    
        }
            
        public void SpawnEvent(string evt, float x, float z) {
            SpawnEvent(evt, x, GetGround(x, z), z);    
        }

        public void SpawnEvent(string evt, Vector3 loc) {
            SpawnEvent(evt, loc.x, loc.x, loc.z);    
        }

        // like an airdrop
        public void SpawnEvent(string evt, float x, float y, float z) {
            GameManager.CreateEntity("events/" + evt, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion()).Spawn(true);
        }

        //Animals: boar, bear, stag, wolf
        public void SpawnAnimal(string name, float x, float y, float z) {
            GameManager.CreateEntity("autospawn/animals/" + name, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion()).Spawn(true);
        }

        //map entities, like a resource node, a tree of even a structure
        public void SpawnMapEntity(string name, float x, float y, float z) {
            GameManager.CreateEntity(name, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion()).SpawnAsMapEntity();
        }
            
        public static World GetWorld()
        {
            if (instance == null)
                instance = new World();
            return instance;
        }

        System.Collections.ArrayList list = new System.Collections.ArrayList();
        public void PrintPrefabs() {
            BaseEntity[] objectsOfType = UnityEngine.Object.FindObjectsOfType<BaseEntity>();
            foreach (BaseEntity baseEntity in objectsOfType)
                if (!list.Contains(baseEntity.sourcePrefab))
                    list.Add(baseEntity.sourcePrefab);
            
            foreach (var s in list)
                Debug.Log(s);
        }
    }
}

