using System;
using UnityEngine;
using System.Timers;

namespace Pluton
{
    public class World
    {

        private static World instance;

        public double ResourceGatherMultiplier = 1.0;
        public Timer freezeTimeTimer;
        private float frozenTime = -1;

        public void AirDrop(float speed = 50f, float height = 400f)
        {
            Vector3 endPos = Vector3Ex.Range(-1f, 1f);
            endPos.y = 0f;
            endPos.Normalize();
            endPos = endPos * 4096f;

            AirDropAt(endPos, speed, height);
        }

        public void AirDropAt(Vector3 position, float speed = 50f, float height = 400f)
        {
            BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", new Vector3(), new Quaternion());
            CargoPlane cp = entity.GetComponent<CargoPlane>();

            Vector3 startPos, endPos;
            float secsToTake;

            startPos = Vector3Ex.Range(-1f, 1f);
            startPos.y = 0f;
            startPos.Normalize();
            startPos = startPos * 4096f;
            startPos.y = height;
            endPos = position + (position - startPos);
            endPos.y = height;
            secsToTake = Vector3.Distance(startPos, endPos) / speed;

            cp.SetFieldValue("startPos", startPos);
            cp.SetFieldValue("endPos", endPos);
            cp.SetFieldValue("secondsToTake", secsToTake);
            cp.SetFieldValue("secondsTaken", 0f);
            cp.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            
            entity.Spawn(true);
        }

        public void AirDropAt(float x, float y, float z, float speed = 50f, float height = 400f)
        {
            AirDropAt(new Vector3(x, y, z));
        }

        public void AirDropAtPlayer(Player player, float speed = 50f, float height = 400f)
        {
            AirDropAt(player.Location);
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

        public BaseEntity SpawnMapEntity(string evt, float x, float z) {
            return SpawnMapEntity(evt, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnMapEntity(string evt, Vector3 loc) {
            return SpawnMapEntity(evt, loc.x, loc.x, loc.z);    
        }
        
        public BaseEntity SpawnAnimal(string name, float x, float z) {
            return SpawnAnimal(name, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnAnimal(string name, Vector3 loc) {
            return SpawnAnimal(name, loc.x, loc.x, loc.z);    
        }
            
        public BaseEntity SpawnEvent(string evt, float x, float z) {
            return SpawnEvent(evt, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnEvent(string evt, Vector3 loc) {
            return SpawnEvent(evt, loc.x, loc.x, loc.z);    
        }

        // like an airdrop
        public BaseEntity SpawnEvent(string evt, float x, float y, float z) {
            BaseEntity ent = GameManager.CreateEntity("events/" + evt, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion());
            ent.Spawn(true);
            return ent;
        }

        //Animals: boar, bear, stag, wolf
        public BaseEntity SpawnAnimal(string name, float x, float y, float z) {
            BaseEntity ent = GameManager.CreateEntity("autospawn/animals/" + name, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion());
            ent.Spawn(true);
            return ent;
        }

        //map entities, like a resource node, a tree of even a structure
        public BaseEntity SpawnMapEntity(string name, float x, float y, float z) {
            BaseEntity ent = GameManager.CreateEntity(name, 
                new UnityEngine.Vector3(x, y, z), 
                new UnityEngine.Quaternion());
            ent.SpawnAsMapEntity();
            return ent;
        }
            
        public float Time {
            get {
                return TOD_Sky.Instance.Cycle.Hour;
            }
            set {
                TOD_Sky.Instance.Cycle.Hour = value;
            }
        }

        public float Timescale {
            get {
                var comp = TOD_Sky.Instance.GetComponent<TOD_Components>();
                var time = comp.GetComponent<TOD_Time>();
                return time.DayLengthInMinutes;
            }
            set {
                var comp = TOD_Sky.Instance.GetComponent<TOD_Components>();
                var time = comp.GetComponent<TOD_Time>();
                time.DayLengthInMinutes = value;
            }
        }

        public void FreezeTime() {
            if (freezeTimeTimer == null) {
                frozenTime = Time;
                freezeTimeTimer = new Timer(10000);
                freezeTimeTimer.Elapsed += new ElapsedEventHandler(this.Freeze);
                freezeTimeTimer.Start();
            }
        }

        private void Freeze(object sender, ElapsedEventArgs e)  {         
            Time = frozenTime;
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

