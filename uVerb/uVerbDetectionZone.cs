using UnityEngine;
using System;

namespace uVerb
{
    /**
     * uVerbDetection Zone : Activate Reverberation when Node is within this Zone.
     * ===========================================================================
     * 
     *      PUBLIC
     *      ======
     *      x, y, z     :   Set in Editor, dictates the size of the detection zone.
     *      Materials   :   Apply absorbant materials here to determine accurate reverb.
     *                          - Floor         (x * z) [Lowest Y]
     *                          - Ceiling       (x * z) [Furthest Y]
     *                          - Left Wall     (z * y) [Lowest X]
     *                          - Right Wall    (z * y) [Furthest X]
     *                          - Front Wall    (x * y) [Lowest Z]
     *                          - Back Wall     (x * y) [Furthest Z]
     *      Debug       :   If enabled, shows Zone without it being selected in Editor.
     *      
     *      PRIVATE
     *      =======
     *      zone        :   Bounds object that acts as the 'trigger'
     *      volume      :   Volume of the zone.
     *      saAvg       :   Surface Area and the absorption coefficients combined.
     *      absorption  :   An array of absorption data for all main materials in the room.
     *      k           :   Constant value for RT60 formula (metres).
     */
    public class uVerbDetectionZone : MonoBehaviour
    {
        public float x, y, z;
        public uVerbEnums.Materials floor;      // [0]
        public uVerbEnums.Materials ceiling;    // [0]
        public uVerbEnums.Materials leftWall;   // [1]
        public uVerbEnums.Materials rightWall;  // [1]
        public uVerbEnums.Materials frontWall;  // [2]
        public uVerbEnums.Materials backWall;   // [2]
        public ReverbType reverbType;
        public bool debug;

        Bounds zone;
        float volume, saAvg, surfaceArea;
        Absorption[] absorption;

        const float k = 0.161f;

        /**
         * Absorption : Structure to contain absorption information.
         */
        public struct Absorption {
            public float Hz125;
            public float Hz250;
            public float Hz500;
            public float Hz1000;
            public float Hz2000;
            public float Hz4000;

            public Absorption (uVerbMaterials.JSONItem data)
            {
                Hz125 = Mathf.Clamp(data.Hz125, 0, 1);
                Hz250 = Mathf.Clamp(data.Hz250, 0, 1);
                Hz500 = Mathf.Clamp(data.Hz500, 0, 1);
                Hz1000 = Mathf.Clamp(data.Hz1000, 0, 1);
                Hz2000 = Mathf.Clamp(data.Hz2000, 0, 1);
                Hz4000 = Mathf.Clamp(data.Hz4000, 0, 1);
            }

            public float AverageAbsorption ()
            {
                float total = Hz125;
                total += Hz250;
                total += Hz500;
                total += Hz1000;
                total += Hz2000;
                total += Hz4000;
                return total / 6; 
            }
        }

        /**
         * ReverbType : Reverberation Type to be applied in this zone.
         */
        public enum ReverbType
        {
            NReverb
        }

        /**
         * Start : On initialisation of the program, set bounds of Detection Zone.
         */
        void Start ()
        {
            uVerbAudioManager manager = FindObjectOfType(typeof(uVerbAudioManager)) as uVerbAudioManager;
            uVerbMaterials materialInfo = manager.GetMaterialData();

            zone = new Bounds(transform.position, new Vector3(x, y, z));
            volume = CalculateVolume();
            absorption = new Absorption[]
            {
                materialInfo.GetMaterial(floor),
                materialInfo.GetMaterial(ceiling),
                materialInfo.GetMaterial(leftWall),
                materialInfo.GetMaterial(rightWall),
                materialInfo.GetMaterial(frontWall),
                materialInfo.GetMaterial(backWall)
            };

            saAvg = CalculateAvgSurfaceAbsorb();

            Debug.Log("Volume is " + volume);
            Debug.Log("Surface Area is " + surfaceArea);
        }

        /**
         * inZone : is the given position in the Detection Zone?
         */
        public bool inZone (Vector3 pos)
        {
            return zone.Contains(pos);
        }

        /**
         *  UpdateZoneArea : Create a new Bounds area for the detection zone.
         */
        public void UpdateZoneArea(Vector3 pos, Vector3 size)
        {
            zone = new Bounds(pos, size);
        }

        /**
         * UpdateZonePosition : Reposition the Zone by the given vector.
         */
        public void UpdateZonePosition(Vector3 pos)
        {
            zone.center = pos;
        }

        /**
         * GetRT60 : Calculates the current RT60 time.
         */
        public float GetAverageRT60 ()
        { 
            float rt60 = (k * volume) / saAvg;
            if (Single.IsNaN(rt60))
                return 0.1f;
            else
                return rt60;
        }

        /**
         * CalculateVolume : Calculate the volume of the zone
         */
        float CalculateVolume ()
        {
            return x * y * z;
        }

        /**
         * CalculateSurfaceAbsorb : Calculate the surface 
         */
        float CalculateAvgSurfaceAbsorb ()
        {
            float sa = 0f;

            float[] surfaces = new float[]
            {
                (z * x),    // Floor + Ceiling
                (z * y),    // Left + Right Walls
                (y * x)     // Front + Back Walls
            };

            for (int i = 0; i < surfaces.Length; i++)
            {
                surfaceArea += surfaces[i] * 2;
                sa += surfaces[i] * absorption[i].AverageAbsorption();
                sa += surfaces[i] * absorption[i + 1].AverageAbsorption();
            }

            return sa;
        }

        /**
         * Editor Functions Below : For displaying the Zone in the editor.
         * If debug is enabled in the inspector, the box will permanently show. 
         */
        void OnDrawGizmosSelected ()
        {
            if (!debug)
            {
                // Draw the box in the editor when selected.
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(x, y, z));
            }
        }

        void OnDrawGizmos ()
        {
            if (debug)
            {
                // Draw the box in the editor when selected.
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(x, y, z));
            }
        }
    }
}

