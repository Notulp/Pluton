using System;
using System.Linq;
using UnityEngine;
using System.Timers;

namespace Pluton
{
    public class World
    {

        private static World instance;

        public double ResourceGatherMultiplier = 1.0d;
        public Timer freezeTimeTimer;
        private float frozenTime = -1;

        public BaseEntity AttachParachute(Player p) {
            return AttachParachute(p.basePlayer);
        }

        public BaseEntity AttachParachute(BaseEntity e) {
            BaseEntity parachute = GameManager.CreateEntity("parachute", default(Vector3), default(Quaternion));
            if (parachute) {
                parachute.SetParent(e, "parachute_attach");
                parachute.Spawn(true);
            }
            return parachute;
        }

        public void AirDrop()
        {
            float speed = UnityEngine.Random.Range(35f, 65f);
            float height = UnityEngine.Random.Range(350f, 550f);

            AirDrop(speed, height);
        }

        public void AirDrop(float speed, float height = 400f)
        {
            float worldSize = (float)(global::World.Size - (global::World.Size / 3));
            Vector3 dropAt = Vector3.zero;

            while (dropAt.x == 0 || dropAt.z == 0)
                dropAt = Vector3Ex.Range(-worldSize, worldSize);

            dropAt.y = 0f;

            AirDropAt(dropAt, speed, height);
        }

        public void AirDropAt(Vector3 position, float speed = 50f, float height = 400f)
        {
            float worldSize = (float)(global::World.Size - (global::World.Size / 7));
            Vector3 zero = Vector3.zero;

            BaseEntity entity = GameManager.CreateEntity("events/cargo_plane", zero, Quaternion.identity);
            CargoPlane cp = entity.GetComponent<CargoPlane>();

            Vector3 startPos = zero, endPos = zero;
            float secsToTake;

            float rand = (float)(worldSize + (worldSize / UnityEngine.Random.Range(-10f, 10f)));

            while (startPos.x == 0 || startPos.z == 0)
                startPos = Vector3Ex.Range(-rand, rand);

            startPos.y = height;
            endPos = position + (position - startPos);
            endPos.y = height;
            secsToTake = Vector3.Distance(startPos, endPos) / speed;

            cp.SetFieldValue("startPos", startPos);
            cp.SetFieldValue("endPos", endPos);
            cp.SetFieldValue("secondsToTake", secsToTake);
            cp.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            
            entity.Spawn(true);

            entity.Invoke("KillMessage", secsToTake - 1);
        }

        public void DropLargeLootBox(Vector3 v3)
        {
            try {
                BaseEntity baseEntity = GameManager.CreateEntity("items/large_woodbox_deployed", v3, default(Quaternion));
                if (baseEntity) {
                    var parachute = GameManager.CreateEntity("parachute", default(Vector3), default(Quaternion));
                    if (parachute) {
                        parachute.SetParent(baseEntity);
                        parachute.Spawn(true);
                    }
                    baseEntity.globalBroadcast = true;
                    baseEntity.Spawn(true);
                    StorageBox sb = baseEntity as StorageBox;
                    sb.inventory.Initialize(null, 16);
                    sb.SetFlag(BaseEntity.Flags.Locked, true);
                    DropUpdate du = baseEntity.gameObject.AddComponent<DropUpdate>();
                    du.parachute = parachute;
                    du.box = sb;
                    du.self = baseEntity;
                    RaycastHit hit;
                    var origin = new Vector3(v3.x, v3.y - 2, v3.z);
                    if (Physics.Raycast(origin, Vector3.down, out hit, 1000,  LayerMask.GetMask(
                        "Deployed", "Ragdoll", "Invisible",
                        "AI", "Player Movement", "Player Interaction",
                        "Game Trace", "Sky", "World", "Player (Server)",
                        "Trigger", "Player Model Renderin", "Grass",
                        "Construction", "Construction Socket", "Terrain",
                        "Guide", "Debris", "Resource", "Construction Trigger"
                    ))) {
                        du.ground = hit.point;
                    }
                }
            } catch (Exception ex) {
                Logger.LogError("[DropLoot] Couldn't drop!");
                Logger.LogError(ex.StackTrace);
            }
        }

        public void AirDropAt(float x, float y, float z, float speed = 50f, float height = 400f)
        {
            AirDropAt(new Vector3(x, y, z), speed, height);
        }

        public void AirDropAtPlayer(Player player, float speed = 50f, float height = 400f)
        {
            AirDropAt(player.Location, speed, height);
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

        public System.Collections.Generic.List<string> GetPrefabNames()
        {
            System.Collections.Generic.Dictionary<uint, string> pool = (System.Collections.Generic.Dictionary<uint, string>)ReflectionExtensions.GetStaticFieldValue(typeof(StringPool), "toString");
            return (from keyvaluepair in pool
                    orderby keyvaluepair.Value ascending
                    select keyvaluepair.Value).ToList<string>();
        }

        public BaseEntity SpawnMapEntity(string name, float x, float z) {
            return SpawnMapEntity(name, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnMapEntity(string name, Vector3 loc) {
            return SpawnMapEntity(name, loc.x, loc.y, loc.z);    
        }

        public BaseEntity SpawnMapEntity(string name, Vector3 loc, Quaternion q) {
            return SpawnMapEntity(name, loc.x, loc.y, loc.z, q);    
        }

        public BaseEntity SpawnMapEntity(string name, float x, float y, float z)
        {
            return SpawnMapEntity(name, x, y, z, Quaternion.identity);
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
        public BaseEntity SpawnMapEntity(string name, float x, float y, float z, Quaternion q) {
            BaseEntity ent = GameManager.CreateEntity(name, 
                new UnityEngine.Vector3(x, y, z), 
                q);
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
            }
            freezeTimeTimer.Start();
        }

        private void Freeze(object sender, ElapsedEventArgs e)  {         
            if (frozenTime != -1)
                Time = frozenTime;
            else
                freezeTimeTimer.Stop();
        }

        public void UnFreezeTime() {
            frozenTime = -1;
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
                if (!list.Contains(baseEntity.LookupPrefabName()))
                    list.Add(baseEntity.LookupPrefabName());
            
            foreach (var s in list)
                Debug.Log(s);
        }
    }
}

