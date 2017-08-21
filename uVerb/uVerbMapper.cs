using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace uVerb
{
    public class uVerbMapper : MonoBehaviour
    {
        public uVerbDetectionZone.ReverbType reverbType;
        uVerbMaterialReader mr = new uVerbMaterialReader();
        public bool DEBUG;
        Vector3 origin;
        List<Vector3> mappedPoints = new List<Vector3>();
        List<uVerbMappingNodeAdv> nodes = new List<uVerbMappingNodeAdv>();
        uVerbMaterials materialInfo;
        float surfaceArea = 0;
        float saAvg;
        const float k = 0.161f;

        public enum RaycastingType
        {
            Physics,
            Mesh
        }
        public RaycastingType raycastingType = RaycastingType.Physics;

        public Vector3 Origin
        {
            get
            {
                return origin;
            }
        }

        public float GetAverageRT60 ()
        {
            float rt60 = (k * Volume) / saAvg;
            if (Single.IsNaN(rt60))
                return 0.1f;
            else
                return (k * Volume) / saAvg;
        }

        public Vector3[] getMapped ()
        {
            return mappedPoints.ToArray();
        }

        public void addPoints (Vector3 pt)
        {
            mappedPoints.Add(pt);
        }

        public void addPoints (Vector3[] pts)
        {
            mappedPoints.AddRange(pts);
        }

        public void addPoints (List<Vector3> pts)
        {
            mappedPoints.AddRange(pts);
        }

        public void mapMaterial (Material mat)
        {
            mr.LogMaterial(mat);
        }

        public float SurfaceArea
        {
            get
            {
                return surfaceArea;
            }

            set
            {
                surfaceArea = value;
            }
        }

        public float Volume
        {
            get
            {
                return mappedPoints.Count;
            }
        }

        void Start ()
        {
            uVerbAudioManager manager = FindObjectOfType(typeof(uVerbAudioManager)) as uVerbAudioManager;
            materialInfo = manager.GetMaterialData();
        }

        void Update ()
        {
            Map();
        }

        void Map ()
        {
            SurfaceArea = 0;

            FindFloor();
            mappedPoints.Clear();
            RaycastHit[] hits;
            hits = Physics.RaycastAll(origin, Vector3.up);
            float distance = -1f;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.transform.gameObject != gameObject)
                {
                    if (distance == -1f) distance = hit.distance;
                    else if (distance > hit.distance) distance = hit.distance;
                }
            }

            distance = Mathf.FloorToInt(distance);
            int j = 0;
            while (j <= distance)
            {
                CreateNode(distance);
                if (j == 0)
                {
                    SurfaceArea += (mappedPoints.Count * 2);
                }
                j++;
            }

            saAvg = CalculateSurfaceAbsorb();
        }

        float CalculateSurfaceAbsorb ()
        {
            float sa = 0f;
            sa = mr.AddSurfaceAbsorbs(materialInfo, SurfaceArea);
            return sa;
        }

        void CreateNode (float yOffset)
        {
            Vector3 v = origin;
            while (yOffset > 0)
            {
                v += Vector3.up;
                yOffset--;
            }
            nodes.Add(new uVerbMappingNodeAdv(this, v));
        }

        public Vector3 convertVector (Vector3 vec)
        {
            float xPos = Mathf.FloorToInt(vec.x);
            xPos += 0.5f;
            float zPos = Mathf.FloorToInt(vec.z);
            zPos += 0.5f;

            return new Vector3(xPos, vec.y, zPos);
        }

        void FindFloor ()
        {
            // Use Mesh raycasting
            if (raycastingType.Equals(RaycastingType.Mesh))
            {
                Debug.LogError("Mesh Raycasting is not implemented yet!");
            }

            // Physics Raycasting by default or selection
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    float yPos = Mathf.Round(hit.point.y);
                    yPos += 0.5f;
                    origin = new Vector3(hit.point.x, yPos, hit.point.z);
                    origin = convertVector(origin);
                }

                else
                {
                    Debug.LogError("We can't find the floor!");
                }
            }
        }

        void OnDrawGizmos ()
        {
            if (DEBUG)
            {
                Gizmos.color = Color.yellow;
                foreach (Vector3 v in mappedPoints)
                {
                    Gizmos.DrawSphere(v, 0.1f);
                }
            }
        }
    }
}