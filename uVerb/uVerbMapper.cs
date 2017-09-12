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
     *      boxcastThreshold:   The higher the number, the lower the precision
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
        [Range(0.1f, 0.5f)]
        public float boxcastThreshold = 0.25f;
        public bool DEBUG;

        uVerbMaterialReader mr = new uVerbMaterialReader();
        uVerbMaterials materialInfo;
        List<Vector3> mappedPoints = new List<Vector3>();
        List<uVerbMappingNodeAdv> nodes = new List<uVerbMappingNodeAdv>();
        float surfaceArea = 0f;
        Vector3 origin;
        float saAvg;
        float extraVolume = 0f;
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
                return rt60;
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

        public void addExtraVolume (float volume)
        {
            extraVolume += volume;
        }

        /**
         * addToSurfaceArea : Two methods to add points to the list of surface area points.
         */
        public void addToSurfaceArea (float amnt)
        {
            surfaceArea += amnt;
        }

        /**
         * mapMaterial : Takes a material to add to the material reader.
         */
        public void mapMaterial (Material mat)
        {
            mr.LogMaterial(mat);
        }

        public void mapMaterial ()
        {
            mr.LogMaterial();
        }

        /**
         * SurfaceArea : Returns the surface area, needs tweaking I think.
         */
        public float SurfaceArea
        {
            get
            {
                return surfaceArea;
            }
        }

        /**
         * Volume : Returns the given volume of a space.
         */
        public float Volume
        {
            get
            {
                return mappedPoints.Count + extraVolume;
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
         * ForceRemap : Forces remapping of room
         */
        public void ForceRemap ()
        {
            origin = convertVector(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            surfaceArea = 0;
            extraVolume = 0;
            nodes.Clear();
            mappedPoints.Clear();
            CreateNode(0);
            saAvg = CalculateSurfaceAbsorb();
            Debug.Log("Reverberation time is " + GetAverageRT60() + "s");
            Debug.Log("Volume is " + Volume + " - extra volume is " + extraVolume);
            Debug.Log("Surface Area is " + SurfaceArea);
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
                Debug.Log("Volume is " + Volume + " - extra volume is " + extraVolume + " - mapped is " + mappedPoints.Count);
                Debug.Log("Surface Area is " + SurfaceArea);
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

            surfaceArea = 0;
            nodes.Clear();
            mappedPoints.Clear();
            extraVolume = 0;
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