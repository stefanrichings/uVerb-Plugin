using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustRaycast : MonoBehaviour {
    static Vector3 edge1 = new Vector3();
    static Vector3 edge2 = new Vector3();
    static Vector3 tVec = new Vector3();
    static Vector3 pVec = new Vector3();
    static Vector3 qVec = new Vector3();

    static float det = 0;
    static float invDet = 0;
    static float u = 0;
    static float v = 0;

    static float epsilon = 0.0000001f;
    public static string intersectionErrorType = "";

    public static bool Raycast (Ray ray, out CustRayHit hit)
    {
        hit = new CustRayHit();
        List<CustRayHit> hits = new List<CustRayHit>();

        hits = INTERNAL_RaycastAll(ray);

        hits = SortResults(hits);
        if (hits.Count > 0)
            hit = hits[0];

        return hits.Count > 0;
    }

    public static CustRayHit[] RaycastAll (Ray ray)
    {
        return INTERNAL_RaycastAll(ray).ToArray();
    }

    public static CustRayHit[] RaycastAll (Ray ray, float dist, LayerMask mask)
    {
        List<CustRayHit> hits = INTERNAL_RaycastAll(ray);
        for (int i = 0; i < hits.Count; i++)
        {
            if (hits[i].distance > dist) hits.RemoveAt(i);
            if ((1 << hits[i].transform.gameObject.layer & mask.value) != 1 << hits[i].transform.gameObject.layer)
                hits.RemoveAt(i);
        }
        return hits.ToArray();
    }

    static List<CustRayHit> INTERNAL_RaycastAll (Ray ray)
    {
        List<CustRayHit> hits = new List<CustRayHit>();
        Octree octree = ObjectDictionary.GetOctree();

        if (octree.bounds.IntersectRay(ray))
        {
            hits = RecurseOctreeBounds(octree, ray);
        }

        hits = SortResults(hits);
        return hits;
    }

    static bool INTERNAL_Raycast (Ray ray, out CustRayHit hit)
    {
        hit = new CustRayHit();
        List<CustRayHit> hits = new List<CustRayHit>();

        Octree octree = ObjectDictionary.GetOctree();

        if (octree.bounds.IntersectRay(ray))
            hits = RecurseOctreeBounds(octree, ray);

        hits = SortResults(hits);
        if (hits.Count > 0)
            hit = hits[0];

        return hits.Count > 0;
    }

    static List<CustRayHit> RecurseOctreeBounds (Octree octree, Ray ray)
    {
        List<CustRayHit> hits = new List<CustRayHit>();
        float dist = 0f;
        Vector2 baryCoord = new Vector2();
        for (int i = 0; i < octree.children.Count; i++)
        {
            if (octree.children[i].bounds.IntersectRay(ray))
            {
                for (int k = 0; k < octree.children[i].triangles.Count; k++)
                {
                    if (TestIntersection(octree.children[i].triangles[k], ray, out dist, out baryCoord))
                    {
                        hits.Add(BuildRaycastHit(octree.children[i].triangles[k], dist, baryCoord));
                    }
                }
                hits.AddRange(RecurseOctreeBounds(octree.children[i], ray));
            }
        }
        return hits;
    }

    static CustRayHit BuildRaycastHit (Triangle hitTriangle, float distance, Vector2 bc)
    {
        CustRayHit returnedHit = new CustRayHit(hitTriangle.trans, distance, bc);
        returnedHit.textureCoord = hitTriangle.uvs.pt0 + ((hitTriangle.uvs.pt1 - hitTriangle.uvs.pt0) * bc.x) + ((hitTriangle.uvs.pt2 - hitTriangle.uvs.pt0) * bc.y);
        returnedHit.point = hitTriangle.pts.pt0 + ((hitTriangle.pts.pt1 - hitTriangle.pts.pt0) * bc.x) + ((hitTriangle.pts.pt2 - hitTriangle.pts.pt0) * bc.y);

        return returnedHit;
    }

    static bool TestIntersection (Triangle tri, Ray ray, out float dist, out Vector2 bc)
    {
        bc = Vector2.zero;
        dist = Mathf.Infinity;
        edge1 = tri.pts.pt1 - tri.pts.pt0;
        edge2 = tri.pts.pt2 - tri.pts.pt0;

        pVec = Vector3.Cross(ray.direction, edge2);
        det = Vector3.Dot(edge1, pVec);
        if (det < epsilon)
        {
            intersectionErrorType = "Failed Epsilon";
            return false;
        }

        tVec = ray.origin - tri.pts.pt0;
        u = Vector3.Dot(tVec, pVec);
        if (u < 0 || u > det)
        {
            intersectionErrorType = "Failed Dot1";
            return false;
        }

        qVec = Vector3.Cross(tVec, edge1);
        v = Vector3.Dot(ray.direction, qVec);
        if (v < 0 || u + v > det)
        {
            intersectionErrorType = "Failed Dot2";
            return false;
        }

        dist = Vector3.Dot(edge2, qVec);
        invDet = 1 / det;
        dist *= invDet;
        bc.x = u * invDet;
        bc.y = v * invDet;
        return true;
    }

    static List<CustRayHit> SortResults (List<CustRayHit> input)
    {
        CustRayHit a = new CustRayHit();
        CustRayHit b = new CustRayHit();
        bool swapped = true;
        while (swapped)
        {
            swapped = false;
            for (int i = 1; i < input.Count; i++)
            {
                if (input[i -1].distance > input[i].distance)
                {
                    a = input[i - 1];
                    b = input[i];
                    input[i - 1] = b;
                    input[i] = a;
                    swapped = true;
                }
            }
        }

        return input;
    }
}

public class CustRayHit
{
    public float distance;
    public Transform transform;
    public Vector2 barycentricCoordinate;
    public Vector2 textureCoord;
    public Vector3 point;

    public CustRayHit ()
    {
        distance = 0f;
        transform = null;
        barycentricCoordinate = Vector2.zero;
        textureCoord = Vector2.zero;
        point = Vector2.zero;
    }

    public CustRayHit (Transform transform, float distance, Vector2 barycentricCoordinate)
    {
        this.distance = distance;
        this.transform = transform;
        this.barycentricCoordinate = barycentricCoordinate;
        textureCoord = Vector2.zero;
        point = Vector2.zero;
    }
}
