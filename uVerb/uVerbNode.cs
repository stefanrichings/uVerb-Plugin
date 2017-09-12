using UnityEngine;

namespace uVerb
{
    /**
     * uVerbNode : Must be applied to any audio source to have Reverb applied.
     * =======================================================================
     * 
     *      PUBLIC
     *      ======
     *      wetMix      :   How much of the Reverb do you want in the DSP?
     *      isActive    :   Is this Node due to apply Reverb?
     *      zoneID      :   Hashcode of the Detection Zone this Node was last in.
     *      
     *      PRIVATE
     *      =======
     *      reverbType  :   The type of Reverb to use.
     *      rt60        :   RT60 time to use.
     *      n           :   The NReverb type.
     */
    [RequireComponent(typeof(AudioSource))]
    public class uVerbNode : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float wetMix = 0.1f;
        public bool isActive = false;
        public long zoneID { get; set; }

        uVerbDetectionZone.ReverbType reverbType;
        NReverb n;
        float rt60;
        
        /**
         * Update : Create Reverb if appropriate and update.
         */
        void Update ()
        {
            switch (reverbType)
            {
                case uVerbDetectionZone.ReverbType.NReverb:
                    if (n == null) n = new NReverb(rt60);
                    n.UpdateParameters();
                    break;
            }
        }

        /**
         * OnAudioFilterRead : Apply Audio Effect to DSP chain.
         */
        void OnAudioFilterRead (float[] data, int channels)
        {
            if (!isActive) return;
            if (channels != 2)
            {
                Debug.LogError("Currently only stereo is supported, given " + channels);
                return;
            }

            switch (reverbType)
            {
                case uVerbDetectionZone.ReverbType.NReverb:
                    if (n != null) data = n.Apply(data, rt60, wetMix);
                    break;
            }
        }

        /**
         * UpdateProperties : Update Reverb settings for detection zone.
         */
        public void UpdateProperties (uVerbDetectionZone zone, uVerbDetectionZone.ReverbType type)
        {
            rt60 = zone.GetAverageRT60();
            Debug.Log(rt60);
            reverbType = type;
        }

        public void UpdateProperties (uVerbMapper mapper, uVerbDetectionZone.ReverbType type)
        {
            rt60 = mapper.GetAverageRT60();
            reverbType = type;
        }

        /**
         * Location : Get current Vector3 postion for Node.
         */
        public Vector3 Location
        {
            get
            {
                return gameObject.transform.position;
            }
        }
    }
}
