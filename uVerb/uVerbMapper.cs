using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace uVerb
{
    /**
     * uVerbMapper : Primary Mapping System for Realtime Calculation
     * =============================================================
     * 
     *      PUBLIC
     *      ======
     *      raycastingType  :   Which type of raycasting to use? Unity Physics or Mesh?
     *      reverbType      :   The reverb type to use
     *      DEBUG           :   Debug capabilities on or off
     *      
     *      PRIVATE
     *      =======
     *      mr              :   Material Reader Object
     *      materialInfo    :   Material info read from JSON file
     *      mappedPoints    :   List of all points mapped
     *      nodes           :   List of nodes to map from
     *      surfaceArea     :   List of surface area points
     *      origin          :   Current origin position of the node (converted)
     *      saAvg           :   Average surface absorption
     *      k               :   Metres constant for conversion
     */
    public class uVerbMapper : MonoBehaviour
    {
        /**
         * RaycastingType : Which Type of Raycasting to use
         */
        public enum RaycastingType
        {
            Physics,
            Mesh
        }
        public RaycastingType raycastingType = RaycastingType.Physics;
        public uVerbDetectionZone.ReverbType reverbType;
        public bool DEBUG;

        uVerbMaterialReader mr = new uVerbMaterialReader();
        uVerbMaterials materialInfo;
        List<Vector3> mappedPoints = new List<Vector3>();
        List<uVerbMappingNodeAdv> nodes = new List<uVerbMappingNodeAdv>();
        List<Vector3> surfaceArea = new List<Vector3>();
        Vector3 origin;
        float saAvg;
        const float k = 0.161f;

        /**
         * Origin : Get current origin point
         */
        public Vector3 Origin
        {
            get
            {
                return origin;
            }
        }

        /**
         * GetAverageRT60 : Gets the average RT60, IsNaN is for safety.
         */
        public float GetAverageRT60 ()
        {
            float rt60 = (k * Volume) / saAvg;
            if (Single.IsNaN(rt60))
                return 0.1f;
            else
                return (k * Volume) / saAvg;
        }

        /**
         * getMapped : Gets an array of the mapped points.
         */
        public Vector3[] getMapped ()
        {
            return mappedPoints.ToArray();
        }

        /**
         * addPoints : Two methods to add points to mapped points.
         */
        public void addPoints (Vector3 pt)
        {
            mappedPoints.Add(pt);
        }

        public void addPoints (Vector3[] pts)
        {
            mappedPoints.AddRange(pts);
        }

        /**
         * addToSurfaceArea : Two methods to add points to the list of surface area points.
         */
        public void addToSurfaceArea (Vector3 pt)
        {
            surfaceArea.Add(pt);
        }

        public void addToSurfaceArea (Vector3[] pts)
        {
            surfaceArea.AddRange(pts);
        }

        /**
         * mapMaterial : Takes a material to add to the material reader.
         */
        public void mapMaterial (Material mat)
        {
            mr.LogMaterial(mat);
        }

        /**
         * SurfaceArea : Returns the surface area, needs tweaking I think.
         */
        public float SurfaceArea
        {
            get
            {
                return surfaceArea.Count;
            }
        }

        /**
         * Volume : Returns the given volume of a space.
         */
        public float Volume
        {
            get
            {
                return mappedPoints.Count;
            }
        }

        /**
         * pointIsMapped : See if point is already mapped
         */
        public bool pointIsMapped (Vector3 v)
        {
            return (mappedPoints.Contains(convertVector(v)));
        }

        /**
         * convertVector : Turns out this is a critical component, normalised points to be regulated. 
         */
        public Vector3 convertVector (Vector3 vec)
        {
            float xPos = Mathf.FloorToInt(vec.x);
            xPos += 0.5f;
            float zPos = Mathf.FloorToInt(vec.z);
            zPos += 0.5f;
            float yPos = Mathf.FloorToInt(vec.y);
            yPos += 0.5f;

            return new Vector3(xPos, yPos, zPos);
        }

        /**
         * Start : Do stuff at Start, after Awake when the Audio Manager is doing its thing
         */
        void Start ()
        {
            uVerbAudioManager manager = FindObjectOfType(typeof(uVerbAudioManager)) as uVerbAudioManager;
            materialInfo = manager.GetMaterialData();
        }

        /**
         * Update : Map every frame
         */
        void Update ()
        {
            Map();
        }

        /**
         * Map : Map the room and only do so when we need to reset
         */
        void Map ()
        {
            bool reset = ResetMap();
            if (reset)
            {
                CreateNode(0);
                saAvg = CalculateSurfaceAbsorb();
                Debug.Log("Reverberation time is " + GetAverageRT60() + "s");
            }
        }

        /**
         * ResetMap : Re-do the origin point, if it isn't in the mapped points, reset the room mapping.
         */
        bool ResetMap ()
        {
            origin = convertVector(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            if (mappedPoints.Contains(origin))
                return false;

            surfaceArea.Clear();
            nodes.Clear();
            mappedPoints.Clear();
            return true;
        }

        /**
         * CalculateSurfaceAbsorb : Get the surface absorbant area
         */
        float CalculateSurfaceAbsorb ()
        {
            float sa = 0f;
            sa = mr.AddSurfaceAbsorbs(materialInfo, SurfaceArea);
            return sa;
        }

        /**
         * CreateNode : Creates a new 'advanced' mapping node, the old one was scrapped so this is now default.
         */
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

        /**
         * OnDrawGizmos : Debug drawing stuff
         */
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