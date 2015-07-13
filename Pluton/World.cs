﻿using System;
using System.Linq;
using UnityEngine;
using System.Timers;

namespace Pluton
{
    public class World : Singleton<World>, ISingleton
    {
        public float ResourceGatherMultiplier = 1.0f;
        public Timer freezeTimeTimer;
        float frozenTime = -1;

        public BaseEntity AttachParachute(Player p)
        {
            return AttachParachute(p.basePlayer);
        }

        public BaseEntity AttachParachute(BaseEntity e)
        {
            BaseEntity parachute = GameManager.server.CreateEntity("parachute");
            if (parachute) {
                parachute.SetParent(e, "parachute_attach");
                parachute.Spawn();
            }
            return parachute;
        }

        public void AirDrop()
        {
            float speed = UnityEngine.Random.Range(30f, 55f);
            float height = UnityEngine.Random.Range(900f, 1000f);
            AirDrop(speed, height);
        }

        public void AirDrop(float speed, float height = 400f)
        {
            BaseEntity baseEntity = GameManager.server.CreateEntity("events/cargo_plane");
            if (baseEntity) {
                baseEntity.Spawn();
            }
            CargoPlane cp = baseEntity.GetComponent<CargoPlane>();
            var start = (Vector3)cp.GetFieldValue("startPos");
            var end = (Vector3)cp.GetFieldValue("endPos");
            start.y = height;
            end.y = height;
            cp.SetFieldValue("secondsToTake", Vector3.Distance(start, end) / speed);
            cp.SetFieldValue("startPos", start);
            cp.SetFieldValue("endPos", end);
        }

        public void AirDropAt(Vector3 position, float speed = 50f, float height = 400f)
        {
            float worldSize = (float)(global::World.Size - (global::World.Size / 7));
            Vector3 zero = Vector3.zero;

            BaseEntity baseEntity = GameManager.server.CreateEntity("events/cargo_plane");
            if (baseEntity) {
                baseEntity.Spawn();
            }
            CargoPlane cp = baseEntity.GetComponent<CargoPlane>();

            Vector3 startPos = zero, endPos = zero;
            float secsToTake;

            float rand = (worldSize * UnityEngine.Random.Range(0.4f, 1.2f));

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
            
            baseEntity.Spawn();
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
            var origin = new Vector3(x, 1000f, z);
            float ground = 0f;
            if (Physics.Raycast(origin, Vector3.down, out hit, Vector3.Distance(origin, new Vector3(origin.x, -100f, origin.z)), 1 << 23)) {
                ground = hit.point.y;
            }
            return ground;
        }

        public float GetGround(Vector3 v3)
        {
            return GetGround(v3.x, v3.z);
        }

        public System.Collections.Generic.List<string> GetPrefabNames()
        {
            var pool = (System.Collections.Generic.Dictionary<uint, string>)ReflectionExtensions.GetStaticFieldValue(typeof(StringPool), "toString");
            return (from keyvaluepair in pool
                             orderby keyvaluepair.Value ascending
                             select keyvaluepair.Value).ToList<string>();
        }

        public BaseEntity SpawnMapEntity(string name, float x, float z)
        {
            return SpawnMapEntity(name, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnMapEntity(string name, Vector3 loc)
        {
            return SpawnMapEntity(name, loc.x, loc.y, loc.z);    
        }

        public BaseEntity SpawnMapEntity(string name, Vector3 loc, Quaternion q)
        {
            return SpawnMapEntity(name, loc.x, loc.y, loc.z, q);    
        }

        public BaseEntity SpawnMapEntity(string name, float x, float y, float z)
        {
            return SpawnMapEntity(name, x, y, z, Quaternion.identity);
        }

        public BaseEntity SpawnAnimal(string name, float x, float z)
        {
            return SpawnAnimal(name, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnAnimal(string name, Vector3 loc)
        {
            return SpawnAnimal(name, loc.x, loc.x, loc.z);    
        }

        public BaseEntity SpawnEvent(string evt, float x, float z)
        {
            return SpawnEvent(evt, x, GetGround(x, z), z);    
        }

        public BaseEntity SpawnEvent(string evt, Vector3 loc)
        {
            return SpawnEvent(evt, loc.x, loc.x, loc.z);    
        }

        // like an airdrop
        public BaseEntity SpawnEvent(string evt, float x, float y, float z)
        {
            BaseEntity ent = GameManager.server.CreateEntity("events/" + evt, 
                                 new Vector3(x, y, z), 
                                 new Quaternion());
            ent.Spawn();
            return ent;
        }

        //Animals: boar, bear, stag, wolf
        public BaseEntity SpawnAnimal(string name, float x, float y, float z)
        {
            BaseEntity ent = GameManager.server.CreateEntity("autospawn/animals/" + name, 
                                 new Vector3(x, y, z), 
                                 new Quaternion());
            ent.Spawn();
            return ent;
        }

        //map entities, like a resource node, a tree of even a structure
        public BaseEntity SpawnMapEntity(string name, float x, float y, float z, Quaternion q)
        {
            BaseEntity ent = GameManager.server.CreateEntity(name, 
                                 new Vector3(x, y, z), 
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

        public void FreezeTime()
        {
            if (freezeTimeTimer == null) {
                frozenTime = Time;
                freezeTimeTimer = new Timer(10000);
                freezeTimeTimer.Elapsed += Freeze;
            }
            freezeTimeTimer.Start();
        }

        void Freeze(object sender, ElapsedEventArgs e)
        {         
            if (frozenTime != -1)
                Time = frozenTime;
            else
                freezeTimeTimer.Stop();
        }

        public void UnFreezeTime()
        {
            frozenTime = -1;
        }

        public void Initialize()
        {
        }

        public bool CheckDependencies()
        {
            return true;
        }

        [Obsolete("World.GetWorld() is obsolete, use World.GetInstance() instead.", false)]
        public static World GetWorld()
        {
            return Instance;
        }

        readonly System.Collections.ArrayList list = new System.Collections.ArrayList();

        public void PrintPrefabs()
        {
            BaseEntity[] objectsOfType = UnityEngine.Object.FindObjectsOfType<BaseEntity>();
            foreach (BaseEntity baseEntity in objectsOfType)
                if (!list.Contains(baseEntity.LookupPrefabName()))
                    list.Add(baseEntity.LookupPrefabName());
            
            foreach (var s in list)
                Debug.Log(s);
        }
    }
}

