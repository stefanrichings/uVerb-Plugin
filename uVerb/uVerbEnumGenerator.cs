using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace uVerb
{
    /**
     * uVerbEnumGenerator : Generator uVerbEnums from materials.json.
     * ==============================================================
     * 
     *      PRIVATE
     *      =======
     *      folderPath  :   Path where first uVerb folder found exists.
     *      filename    :   constant name of the filename to generate.
     *      json        :   constant name of the json to read.
     */
    public class uVerbEnumGenerator
    {
        string folderPath = null;
        const string filename = "uVerbEnums.cs";
        const string json = "materials.json";

        /**
         * GenerateClass : Main function being called to generate the class.
         */
        public void GenerateClass()
        {
            SearchDirectory(Application.dataPath);
            if (folderPath != null)
            {
                try
                {
                    File.WriteAllLines(folderPath += filename, GenerateLines());
                    Debug.Log("File was written successfully! Refreshing Assets...");
                    AssetDatabase.Refresh();
                }

                catch (Exception e)
                {
                    Debug.LogError("Cannot create file: " + e);
                }
            }

            else
            {
                Debug.LogError("No folder name uVerb found, cannot generate class!");
            }
        }

        /**
         * SearchDirectory : Find the first instance of a uVerb folder to create the class in.
         */
        void SearchDirectory(string path)
        {
            string[] dirs = Directory.GetDirectories(path, "uVerb");
            if (dirs.Length > 0) folderPath = dirs[0] += "\\";
            else
            {
                string[] subdirs = Directory.GetDirectories(path);
                foreach(string dir in subdirs)
                {
                    if (folderPath == null)
                        SearchDirectory(dir);
                }
            }
        }

        /**
         * GenerateLines : Generate the file contents.
         */
        string[] GenerateLines()
        {
            List<string> lines = new List<string>();
            string padding = "            ";

            lines.Add("namespace uVerb {");
            lines.Add("    public class uVerbEnums {");
            lines.Add("        public enum Materials {");

            string jsonFile = Path.Combine(Application.streamingAssetsPath, json);
            if (File.Exists(jsonFile))
            {
                string dataAsJSON = File.ReadAllText(jsonFile);
                uVerbMaterials loadedData = JsonUtility.FromJson<uVerbMaterials>(dataAsJSON);

                string[] names = loadedData.GetMaterialNames();
                StringBuilder sb = new StringBuilder();
                foreach (string name in names)
                {
                    if (!names.First().Equals(name)) sb.Append(",\r\n");
                    sb.Append(padding);
                    sb.Append(name);
                }

                lines.Add(sb.ToString());
            }

            else
            {
                Debug.LogError("Unable to load data from " + jsonFile);
            }

            lines.Add("        }");
            lines.Add("    }");
            lines.Add("}");

            return lines.ToArray();
        }
    }
}

