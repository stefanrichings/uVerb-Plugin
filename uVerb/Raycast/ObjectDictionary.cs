using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDictionary : MonoBehaviour {
    public Octree octree;
    public static ObjectDictionary od;

    int octreeDepth = 3;
    int objectsPerChunk = 1000;

    void Awake ()
    {
        od = this;
        Init(GetSceneBounds());
    }

    public void Init (Bounds bounds)
    {
        if (od == null) od = this;
        octree = new Octree(bounds, octreeDepth);
        StartCoroutine(PopulateOctree());
    }

    public static Octree GetOctree ()
    {
        return od.octree;
    }

    public static Bounds GetSceneBounds ()
    {
        Bounds scene = new Bounds(Vector3.zero, Vector3.zero);
        Transform[] transforms = FindObjectsOfType<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            scene.Encapsulate(transforms[i].position);
        }

        scene.size *= 2;
        return scene;
    }

    IEnumerator PopulateOctree ()
    {
        // Possible take an argument to look for all objects or objects of a given 'uVerbWall'?
        GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];

        GameObject current;
        Triangle[] currentTris = new Triangle[] { };
        MeshFilter currentFilter = null;
        Octree finalNode;
        for (int i = 0; i < objects.Length; i++)
        {
            current = objects[i];
            if (current == null || current.name.Contains("Combined Mesh") || current.name == null) continue;

            currentFilter = current.GetComponent<MeshFilter>();
            if (!currentFilter) continue;

            currentTris = new Triangle[] { };
            currentTris = GetTriangles(current);
            for (int k = 0; k < currentTris.Length; k++)
            {
                finalNode = octree.IndexTriangle(currentTris[k]);
                finalNode.AddTriangle(currentTris[k]);
            }

            if (i % objectsPerChunk == 1)
            {
                yield return 0;
            }
        }

        Debug.Log("Created GameObject Database");
        Debug.Log("Total indexed triangles: " + GetTriangleCount(octree));
    }

    int GetTriangleCount(Octree o)
    {
        int count = o.triangles.Count;
        foreach (Octree oct in o.children)
        {
            count += GetTriangleCount(oct);
        }

        return count;
    }

    Triangle[] GetTriangles (GameObject go)
    {
        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
        int[] vIndex = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        List<Triangle> triangles = new List<Triangle>();

        int i = 0;
        while (i < vIndex.Length)
        {
            MeshData.Points p = new MeshData.Points(verts[vIndex[i]], verts[vIndex[i + 1]], verts[vIndex[i + 2]]);
            MeshData.UVs u = new MeshData.UVs(uvs[vIndex[i]], uvs[vIndex[i + 1]], uvs[vIndex[i + 2]]);
            triangles.Add (new Triangle(p, u, go.transform));
            i += 3;
        }

        return triangles.ToArray();
    }

    void OnDestroy ()
    {
        Debug.Log("Mem Before Clear: " + System.GC.GetTotalMemory(true) / 1024f / 1024f);
        octree.Clear();
        octree = null;
        Destroy(od);
        Debug.Log("Mem After Clear: " + System.GC.GetTotalMemory(true) / 1024f / 1024f);
    }
}

public class MeshData
{
    public struct Points
    {
        public Vector3 pt0;
        public Vector3 pt1;
        public Vector3 pt2;

        public Points (Vector3 pt0, Vector3 pt1, Vector3 pt2)
        {
            this.pt0 = pt0;
            this.pt1 = pt1;
            this.pt2 = pt2;
        }
    }

    public struct UVs
    {
        public Vector2 pt0;
        public Vector2 pt1;
        public Vector2 pt2;

        public UVs (Vector2 pt0, Vector2 pt1, Vector2 pt2)
        {
            this.pt0 = pt0;
            this.pt1 = pt1;
            this.pt2 = pt2;
        }
    }
}
