using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uVerb
{
    public class uVerbMappingNodeAdv
    {
        uVerbMapper parent;
        List<Vector3> pointsToMap = new List<Vector3>();
        Vector3[] dirs = new Vector3[]
        {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
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
            parent.addPoints(origin);
            pointsToMap.Add(origin);

            bool mapping = true;
            while (mapping)
            {
                Vector3[] temp = pointsToMap.ToArray();

                for (int i=0; i < temp.Length; i++)
                {
                    MapPoint(temp[i]);
                    pointsToMap.Remove(temp[i]);
                }

                if (pointsToMap.Count == 0) mapping = false;
            }
        }

        void MapPoint (Vector3 p)
        {
            RaycastHit hit;
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!isPointMapped(p + dirs[i]))
                {
                    float extents = parent.boxcastThreshold;
                    if (Physics.BoxCast(p, new Vector3(extents,extents,extents), dirs[i], out hit))
                    {
                        if (hit.distance > 1)
                        {
                            parent.addPoints(p + dirs[i]);
                            pointsToMap.Add(p + dirs[i]);
                        }

                        else
                        {
                            parent.addToSurfaceArea(p + dirs[i]);
                            Renderer rend = hit.transform.gameObject.GetComponent<Renderer>();
                            if (rend != null) parent.mapMaterial(rend.material);
                        }
                    }
                }
            }
        }

        bool isPointMapped (Vector3 p)
        {
            return parent.pointIsMapped(p);
        }
    }
}
