using System.Collections.Generic;
using UnityEngine;

public class Octree {
    public List<Octree> children;
    public Octree parent;
    public Bounds bounds;
    public List<Triangle> triangles;

    public Octree ()
    {
        children = new List<Octree>();
        triangles = new List<Triangle>();
        parent = null;
    }

    public Octree (Bounds parentBounds, int generations)
    {
        bounds = parentBounds;
        children = new List<Octree>();
        triangles = new List<Triangle>();
        parent = null;
        CreateChildren(this, generations);
    }

    public Octree IndexTriangle (Triangle tri)
    {
        return IndexTriangle(this, tri);
    }

    public Octree IndexTriangle (Octree parentNode, Triangle triangle)
    {
        float minX = Mathf.Min(triangle.pts.pt0.x, Mathf.Min(triangle.pts.pt1.x, triangle.pts.pt2.x));
        float minY = Mathf.Min(triangle.pts.pt0.y, Mathf.Min(triangle.pts.pt1.y, triangle.pts.pt2.y));
        float minZ = Mathf.Min(triangle.pts.pt0.z, Mathf.Min(triangle.pts.pt1.z, triangle.pts.pt2.z));

        float maxX = Mathf.Max(triangle.pts.pt0.x, Mathf.Max(triangle.pts.pt1.x, triangle.pts.pt2.x));
        float maxY = Mathf.Max(triangle.pts.pt0.y, Mathf.Max(triangle.pts.pt1.y, triangle.pts.pt2.y));
        float maxZ = Mathf.Max(triangle.pts.pt0.z, Mathf.Max(triangle.pts.pt1.z, triangle.pts.pt2.z));

        Octree finalNode = null;
        Octree currentNode = parentNode;
        while (currentNode != null && finalNode == null) {
            float boundsCenterX = currentNode.bounds.center.x;
            float boundsCenterY = currentNode.bounds.center.y;
            float boundsCenterZ = currentNode.bounds.center.z;

            if ((minX < boundsCenterX && maxX >= boundsCenterX) || (minY < boundsCenterY && maxY >= boundsCenterY) || (minZ < boundsCenterZ && maxZ >= boundsCenterZ)) {
                finalNode = currentNode;
            } else {
                if (currentNode.children != null && currentNode.children.Count > 0) {
                    int childIndex = 0;
                    if (minX < boundsCenterX)
                        childIndex |= 4;
                    if (minY < boundsCenterY)
                        childIndex |= 2;
                    if (minZ < boundsCenterZ)
                        childIndex |= 1;

                    currentNode = currentNode.children[childIndex];
                } else {
                    finalNode = currentNode;
                }
            }
        }

        return finalNode;
    }

    public bool AddTriangle (Triangle tri)
    {
        triangles.Add(tri);
        return true;
    }

    public bool ContainsTriangle (Triangle tri)
    {
        return bounds.Contains(tri.pts.pt0) &&
               bounds.Contains(tri.pts.pt1) &&
               bounds.Contains(tri.pts.pt2);
    }

    public void Clear ()
    {
        int total = ClearOctree(this);
        Debug.Log("Total Nodes Cleared: " + total);
    }

    protected int ClearOctree (Octree o)
    {
        int count = 0;
        for (int i = 0; i < o.children.Count; i++)
        {
            count += ClearOctree(o.children[i]);
        }
        o.triangles.Clear();
        o.triangles.TrimExcess();
        o.parent = null;
        o.children.Clear();
        o.children.TrimExcess();
        count++;
        return count;
    }

    protected void CreateChildren (Octree parent, int generations)
    {
        children = new List<Octree>();
        Vector3 c = parent.bounds.center;
        float u = parent.bounds.extents.x * 0.5f;
        float v = parent.bounds.extents.y * 0.5f;
        float w = parent.bounds.extents.z * 0.5f;
        Vector3 childrenSize = parent.bounds.extents;
        Vector3[] childrenCenters =
        {
            new Vector3(c.x + u, c.y + v, c.z + w),
            new Vector3(c.x + u, c.y + v, c.z - w),
            new Vector3(c.x + u, c.y - v, c.z + w),
            new Vector3(c.x + u, c.y - v, c.z - w),
            new Vector3(c.x - u, c.y + v, c.z + w),
            new Vector3(c.x - u, c.y + v, c.z - w),
            new Vector3(c.x - u, c.y - v, c.z + w),
            new Vector3(c.x - u, c.y - v, c.z - w)
        };

        for (int i = 0; i < childrenCenters.Length; i++)
        {
            Octree o = new Octree();
            o.parent = parent;
            o.bounds = new Bounds(childrenCenters[i], childrenSize);
            children.Add(o);
            if (generations > 0)
                o.CreateChildren(o, generations - 1);
        }
    }
}
