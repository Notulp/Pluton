using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pluton
{
    public class Zone2D : MonoBehaviour
    {
        public Mesh zoneMesh;
        public MeshFilter zoneMeshFilter;
        public MeshCollider zoneCollider;

        public string Name = "uninitalized";

        public List<Vector3> Verts;

        public static float min = -50f;
        public static float max = (float)global::World.Height;

        public int[] Tris;
        public int TrisCount = 0;

        public void ResetTris()
        {
            Tris = new int[0];
            TrisCount = 0;
        }

        public void ComputeAllTris()
        {
            TrisCount = 0;

            int capacity = (Verts.Count - 2) * 6;
            Tris = new int[capacity];

            ComputeSideTris();
            ComputeTopTris();
            ComputeBottomTris();
        }

        public void ComputeSideTris()
        {
            int length = Verts.Count;
            for (int i = 0; i < length; i++) {
                Tris[TrisCount] = i;
                TrisCount += 1;
                if (i == (length - 1)) {
                    Tris[TrisCount] = 0;
                    Tris[TrisCount + 1] = 1;
                    TrisCount += 2;
                } else if (i == (length - 2)) {
                    Tris[TrisCount] = i + 1;
                    Tris[TrisCount + 1] = 0;
                    TrisCount += 2;
                } else {
                    Tris[TrisCount] = i + 1;
                    Tris[TrisCount + 1] = i + 2;
                    TrisCount += 2;
                }
            }
        }

        public void ComputeTopTris()
        {
            int length = Verts.Count;
            for (int i = 2; i <= length - 4; i += 2) {
                Tris[TrisCount] = 0;
                Tris[TrisCount + 1] = i;
                Tris[TrisCount + 2] = i + 2;
                TrisCount += 3;
            }
        }

        public void ComputeBottomTris()
        {
            int length = Verts.Count;
            for (int i = 3; i <= length - 3; i += 2) {
                Tris[TrisCount] = 1;
                Tris[TrisCount + 1] = i;
                Tris[TrisCount + 2] = i + 2;
                TrisCount += 3;
            }
        }

        void Awake()
        {
            zoneMeshFilter = gameObject.AddComponent<MeshFilter>();
            zoneMeshFilter.mesh = new Mesh();
            zoneMesh = zoneMeshFilter.sharedMesh;
            zoneCollider = gameObject.AddComponent<MeshCollider>();

            Verts = new List<Vector3>();
        }

        public void UpdateMesh()
        {
            zoneMesh.Clear();
            zoneMesh.vertices = Verts.ToArray();
            zoneCollider.convex = true;

            ComputeAllTris();

            zoneMesh.triangles = Tris;
            zoneMesh.RecalculateBounds();
            zoneMesh.Optimize();

            gameObject.GetComponent<MeshFilter>().mesh = zoneMesh;
            zoneCollider.isTrigger = true;
            zoneCollider.sharedMesh = null;
            zoneCollider.sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
        }

        public void AddPoint(float x, float z)
        {
            AddPoint(new Vector3(x, 0, z));
        }

        public void AddPoint(float x, float y, float z)
        {
            AddPoint(new Vector3(x, y, z));
        }

        public void AddPoint(Vector3 p)
        {
            p.y = min;
            Verts.Add(p);
            p.y = max;
            Verts.Add(p);
        }

        public void Clear()
        {
            Verts = new List<Vector3>();
            UpdateMesh();
        }

        public bool Contains(float x, float z)
        {
            return Contains(new Vector3(x, 0, z));
        }

        public bool Contains(float x, float y, float z)
        {
            return Contains(new Vector3(x, y, z));
        }

        public bool Contains(Vector3 v3)
        {
            return gameObject.GetComponent<MeshCollider>().bounds.Contains(v3);
        }

        public void Draw()
        {
            ComputeAllTris();
            for (int i = 0; i < Tris.Length; i += 3) {
                DrawLine(Verts[Tris[i]], Verts[Tris[i + 1]], Color.red);
                DrawLine(Verts[Tris[i]], Verts[Tris[i + 2]], Color.red);
                DrawLine(Verts[Tris[i + 1]], Verts[Tris[i + 2]], Color.red);
            }
        }

        public void DrawLine(Vector3 From, Vector3 To, Color color)
        {
            ConsoleSystem.Broadcast("ddraw.arrow", new object[] {
                60, color, From, To, 0.1f
            });
        }

        public SerializedZone2D Serialize()
        {
            Debug.LogWarning("Serializing '" + Name + "' zone.");
            SerializedZone2D result = new SerializedZone2D();
            result.Tris = Tris;
            result.TrisCount = TrisCount;
            result.Verts = Verts.Select(x => x.Serialize()).ToList();
            result.Name = Name;
            return result;
        }
    }
}

