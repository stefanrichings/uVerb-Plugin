using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace uVerb
{
    /**
     * uVerbRaycaster : Map a Mesh Bounds to use a detection zone.
     * ===========================================================
     * 
     *      PUBLIC
     *      ======
     *      resolution  :   Number of points to detect.
     */
    public class uVerbRaycaster : MonoBehaviour
    {
        [Range(16, 128)]
        public int resolution;

        List<Vector3> points;
        bool completed = false;

        public void Start ()
        {
            StartCoroutine(BuildZone());
        }

        IEnumerator BuildZone ()
        {
            yield return new WaitUntil(() => ObjectDictionary.GetOctree() != null);
            DrawRays();
        }

        /**
         * DrawRays : Draws the Rays out.
         */
        void DrawRays()
        {
            points = new List<Vector3>();
            foreach (var dir in GetRayDirections(resolution))
            {
                Ray ray = new Ray(transform.position, dir);
                Debug.DrawRay(ray.origin, ray.direction);
                CustRayHit hit;

                if (CustRaycast.Raycast(ray, out hit))
                {
                    points.Add(hit.point);
                }
            }

            completed = true;
            Debug.Log("There are " + points.Count + " points");
        }

        void OnDrawGizmos ()
        {
            if (completed)
            {
                foreach (var p in points)
                {
                    Gizmos.DrawSphere(p, 0.2f);
                }
            }
        }

        /**
         * GetRayDirections : Gets an evenly distributed set of Raycasts based on resolution.
         */
        Vector3[] GetRayDirections (int directions)
        {
            var points = new Vector3[directions];
            var increment = Mathf.PI * (3 - Mathf.Sqrt(5));
            var offset = 2f / directions;

            foreach (var k in Enumerable.Range(0, directions))
            {
                var y = k * offset - 1 + (offset / 2);
                var r = Mathf.Sqrt(1 - y * y);
                var phi = k * increment;

                var x = (Mathf.Cos(phi) * r);
                var z = (Mathf.Sin(phi) * r);
                points[k] = new Vector3(x, y, z);
            }

            return points;
        }
    }
}

