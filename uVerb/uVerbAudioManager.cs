using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace uVerb
{
    /**
     * uVerbAudioManager : Primary Audio Manager for the uVerb Plugin.
     * ===============================================================
     * 
     *      PUBLIC
     *      ======
     *      manager     :   Singleton object of Manager.
     *      
     *      PRIVATE
     *      =======
     *      loadedData  :   uVerbMaterials object of material information loaded from JSON.
     *      nodes       :   List of all Node Objects in Scene.
     *      zones       :   List of all Detection Zones in Scene.
     *      jsonFile    :   Constant variable for materials.json.
     *      buffer      :   (Buffer Length), size of buffer.
     *      numBuffers  :   Number of buffers.
     *      sampleRate  :   Sample Rate of the project.
     */
    public class uVerbAudioManager : MonoBehaviour
    {
        public enum ReverbType
        {
            Boxes,
            Realtime
        }
        public ReverbType type = ReverbType.Realtime;
        public static uVerbAudioManager manager = null;

        uVerbMaterials loadedData;
        List<uVerbNode> nodes;
        List<uVerbDetectionZone> zones;
        uVerbMapper mapper;
        const string jsonFile = "materials.json";
        int bufferLength, numBuffers;
        int sampleRate;

        /**
         * Awake : On Initialisation, set up Singleton.
         */
        void Awake ()
        {
            if (manager == null)
            {
                manager = this;
                LoadJSONData();
            }
            else if (manager != null)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            GetNodesAndZones();

            AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
            sampleRate = AudioSettings.outputSampleRate;
        }

        /**
         * Update : Do work each frame, you know what this does.
         */
        void Update ()
        {
            if (type.Equals(ReverbType.Boxes))
            {
                CheckZones();
            }

            else
            {
                DoRealtime();
            }
        }

        void DoRealtime ()
        {
            foreach (uVerbNode node in nodes)
            {
                node.isActive = true;
                node.UpdateProperties(mapper, mapper.reverbType);
            }
        }

        /**
         * CheckZones : Foreach Node, check if it is in Zone and update properties.
         */
        void CheckZones ()
        {
            foreach (uVerbNode node in nodes)
            {
                node.isActive = false;
                foreach (uVerbDetectionZone zone in zones)
                {
                    if (zone.inZone(node.Location))
                    {
                        node.isActive = true;
                        if (!node.zoneID.Equals(zone.GetHashCode()))
                        {
                            node.zoneID = zone.GetHashCode();
                            node.UpdateProperties(zone, zone.reverbType);
                        }
                    }
                }
            }
        }

        /**
         * GetNodesAndZones : Find all Nodes and Zones in scene.
         */
        void GetNodesAndZones ()
        {
            nodes = new List<uVerbNode>();
            uVerbNode[] nodeArray = FindObjectsOfType(typeof(uVerbNode)) as uVerbNode[];
            foreach (uVerbNode node in nodeArray)
            {
                nodes.Add(node);
            }

            zones = new List<uVerbDetectionZone>();
            uVerbDetectionZone[] zoneArray = FindObjectsOfType(typeof(uVerbDetectionZone)) as uVerbDetectionZone[];
            foreach (uVerbDetectionZone zone in zoneArray)
            {
                zones.Add(zone);
            }

            mapper = FindObjectOfType(typeof(uVerbMapper)) as uVerbMapper;
        }

        /**
         * LoadJSONData : Load data from materials.json in Streaming Assets.
         */
        void LoadJSONData ()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, jsonFile);
            if (File.Exists(filePath))
            {
                string dataAsJSON = File.ReadAllText(filePath);
                loadedData = JsonUtility.FromJson<uVerbMaterials>(dataAsJSON);
            }

            else
            {
                Debug.LogError("Unable to load data from " + jsonFile);
            }
        }

        /**
         * GetMaterialData : Get the data loaded from materials.json
         */
        public uVerbMaterials GetMaterialData ()
        {
            return loadedData;
        }

        /**
         * GetBufferSize : Get the size of the project's buffer length.
         */
        public int GetBufferSize ()
        {
            return bufferLength;
        }

        /**
         * GetNumberOfBuffers : Get the number of buffers
         */
        public int GetNumberOfBuffers()
        {
            return numBuffers;
        }

        public int GetSampleRate()
        {
            return sampleRate;
        }
    }

    /**
     * MathUtil : Extra Maths Functions
     * ================================
     *      
     *      Written by Keijiro (https://github.com/keijiro)
     */
    public class MathUtil
    {
        /**
         * IsPrime : Is the number a prime number?
         */
        public static bool IsPrime (int number)
        {
            if ((number & 1) == 1)
            {
                var upto = (int)Mathf.Sqrt(number);
                for (var i = 3; i <= upto; i += 2)
                {
                    if (number % i == 0)
                        return false;
                }
                return true;
            }
            else
            {
                return (number == 2);
            }
        }
    }
}
