using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Pluton
{
    [Serializable]
    public class SerializedZone2D
    {
        public string Name;
        public int[] Tris;
        public int TrisCount;
        public List<SerializedVector3> Verts;

        public Zone2D ToZone2D()
        {
            try {
                GameObject obj = new GameObject(Name);
                var gobj = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
                var zone = gobj.AddComponent<Zone2D>();
                zone.Name = Name;
                zone.Tris = Tris;
                zone.TrisCount = TrisCount;
                zone.Verts = Verts.Select(x => x.ToVector3()).ToList();
                Util.GetInstance().zones.Add(Name, zone);
                return zone;
            } catch (Exception ex) {
                Logger.LogException(ex);
                return null;
            }
        }
    }
}

