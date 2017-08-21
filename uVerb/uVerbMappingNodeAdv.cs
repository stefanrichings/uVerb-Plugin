using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uVerb
{
    public class uVerbMappingNodeAdv
    {
        uVerbMapper parent;
        List<Vector3> points = new List<Vector3>();
        List<Vector3> pointsToMap = new List<Vector3>();
        Vector3[] dirs = new Vector3[]
        {
        Vector3.forward, Vector3.back, Vector3.left, Vector3.right
        };
        Vector3 origin;

        public uVerbMappingNodeAdv (uVerbMapper parent, Vector3 origin)
        {
            this.parent = parent;
            this.origin = origin;
            Map();
        }

        void Map ()
        {
            points.Add(origin);
            pointsToMap.Add(origin);

            bool mapping = true;
            while (mapping)
            {
                Vector3[] temp = pointsToMap.ToArray();
                foreach (Vector3 p in temp)
                {
                    MapPoint(p);
                    pointsToMap.Remove(p);
                }

                if (pointsToMap.Count == 0) mapping = false;
            }

            parent.addPoints(points);
        }

        void MapPoint (Vector3 p)
        {
            RaycastHit hit;
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!isPointMapped(p + dirs[i]))
                {
                    if (Physics.Raycast(p, dirs[i], out hit))
                    {
                        if (hit.distance > 1)
                        {
                            points.Add(p + dirs[i]);
                            pointsToMap.Add(p + dirs[i]);
                        }

                        else
                        {
                            parent.SurfaceArea++;
                            Renderer rend = hit.transform.gameObject.GetComponent<Renderer>();
                            parent.mapMaterial(rend.material);
                        }
                    }
                }
            }
        }

        bool isPointMapped (Vector3 p)
        {
            return points.Contains(p);
        }
    }

}
