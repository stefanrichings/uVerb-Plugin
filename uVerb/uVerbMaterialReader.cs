using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uVerb
{
    public class uVerbMaterialReader
    {
        public struct MaterialLog
        {
            public MaterialLog (string name)
            {
                this.name = name;
                assigned = 0;
            }

            public string name;
            public uVerbEnums.Materials assigned;
            
            public void Assign ()
            {
                assigned = approximateMaterial();
            }

            // Need to do the 'learning' bits to approximate the material
            uVerbEnums.Materials approximateMaterial ()
            {
                return 0;
            }
        }

        Dictionary<string, int> materials = new Dictionary<string, int>();
        List<MaterialLog> logged = new List<MaterialLog>();
        int totalHits;

        public void LogMaterial (Material mat)
        {
            if (materials.ContainsKey(mat.name))
            {
                materials[mat.name]++;
                totalHits++;
            }

            else
            {
                materials.Add(mat.name, 1);
                MaterialLog ml = new MaterialLog(mat.name);
                ml.Assign();
                logged.Add(ml);
                totalHits++;
            }
        }

        public float AddSurfaceAbsorbs (uVerbMaterials materialInfo, float surfaceArea)
        {
            float sa = 0f;

            foreach (MaterialLog mat in logged)
            {
                int count;
                materials.TryGetValue(mat.name, out count);
                float perc = (float)count / (float)totalHits;
                uVerbDetectionZone.Absorption abs = materialInfo.GetMaterial(mat.assigned);
                sa += (surfaceArea * perc) * abs.AverageAbsorption();
            }

            return sa;
        }

        public void printMaterials ()
        {
            Debug.Log("There are a total of " + materials.Count + " separate materials");
            foreach (KeyValuePair<string, int> value in materials)
            {
                Debug.Log(value.Key + " has a material count of " + value.Value);
            }
        }
    }
}
