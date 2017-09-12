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
                            parent.addExtraVolume(hit.distance);
                            parent.addToSurfaceArea(getSurfaceArea(p, dirs[i]));
                            Renderer rend = hit.transform.gameObject.GetComponent<Renderer>();
                            if (rend != null)
                            {
                                parent.mapMaterial(rend.material);
                            }

                            else
                            {
                                parent.mapMaterial();
                            }
                        }
                    }
                }
            }
        }

        float getSurfaceArea (Vector3 p, Vector3 dir)
        {
            RaycastHit hit;
            float extents = parent.boxcastThreshold;
            float surface = 1f;
            Dictionary<Vector3, Vector3[]> index = new Dictionary<Vector3, Vector3[]>
            {
                { Vector3.forward, new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right } },
                { Vector3.back, new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right } },
                { Vector3.left, new Vector3[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back } },
                { Vector3.right, new Vector3[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back } },
                { Vector3.up, new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right } },
                { Vector3.down, new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right } }
            };

            Vector3[] arr;
            index.TryGetValue(dir, out arr);
            for (int i = 0; i < arr.Length; i++)
            {
                if (Physics.Raycast(p, arr[i], out hit))
                {
                    if (hit.distance < 0.75f)
                    {
                        surface++;
                        Renderer rend = hit.transform.gameObject.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            parent.mapMaterial(rend.material);
                        }

                        else
                        {
                            parent.mapMaterial();
                        }
                    }
                }
            }

            return surface;
        }

        bool isPointMapped (Vector3 p)
        {
            return parent.pointIsMapped(p);
        }
    }
}
