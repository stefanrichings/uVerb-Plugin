using System.Collections.Generic;

namespace uVerb
{
    /**
     * uVerbMaterials : Class to store Material information at runtime.
     * ================================================================
     * 
     *      PUBLIC
     *      ======
     *      materials   :   Array of information from JSON file.
     */
    [System.Serializable]
    public class uVerbMaterials
    {
        /**
         * JSONItem : Struct to read in information from materials.json
         */
        [System.Serializable]
        public struct JSONItem
        {
            public string name;
            public float Hz125;
            public float Hz250;
            public float Hz500;
            public float Hz1000;
            public float Hz2000;
            public float Hz4000;
        }

        public JSONItem[] materials;

        /**
         * GetMaterialNames : Return array of all names in materials.
         */
        public string[] GetMaterialNames ()
        {
            List<string> names = new List<string>();
            foreach (JSONItem item in materials)
            {
                names.Add(item.name);
            }

            return names.ToArray();
        }

        /**
         * GetMaterial : Get the corresponding material structure.
         */
        public uVerbDetectionZone.Absorption GetMaterial (uVerbEnums.Materials material)
        {
            return new uVerbDetectionZone.Absorption(materials[(int)material]);
        }
    }
}

