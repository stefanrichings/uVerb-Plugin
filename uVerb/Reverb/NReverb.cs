using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uVerb
{
    public class NReverb
    {
        DelayBuffer[] allPassFilters;
        DelayBuffer[] combFilters;
        float allPassCoeff = 0.7f;
        float[] combCoeff;
        float lowpassState;
        float rt60;

        public NReverb (float t60)
        {
            rt60 = t60;
            allPassFilters = new DelayBuffer[6];
            combFilters = new DelayBuffer[6];
            combCoeff = new float[6];

            int[] delays =
            {
                1433, 1601, 1867, 2053, 2251, 2399,
                347, 113, 37, 59, 53, 43
            };

            float scaler = uVerbAudioManager.manager.GetSampleRate() / 25641.0f;
            for (int i = 0; i < delays.Length; i++)
            {
                var delay = Mathf.FloorToInt(scaler * delays[i]);
                if ((delay & 1) == 0) delay++;
                while (!MathUtil.IsPrime(delay)) delay += 2;
                delays[i] = delay;
            }

            for (int i = 0; i < 6; i++)
            {
                combFilters[i] = new DelayBuffer(delays[i]);
            }

            for (int i = 0; i < 6; i++)
            {
                allPassFilters[i] = new DelayBuffer(delays[i + 6]);
            }

            UpdateParameters();
        }

        public void UpdateParameters ()
        {
            float scaler = -3.0f / (rt60 * uVerbAudioManager.manager.GetSampleRate());
            for (int i = 0; i < 6; i++)
            {
                combCoeff[i] = Mathf.Pow(10.0f, scaler * combFilters[i].Length);
            }
        }

        public float[] Apply (float[] data, float t60, float mix)
        {
            if (!t60.Equals(rt60)) rt60 = t60;

            for (int offset = 0; offset < data.Length; offset+=2)
            {
                var input = 0.5f * (data[offset] + data[offset + 1]);

                var temp0 = 0.0f;
                for (int i = 0; i < 6; i++)
                    temp0 += combFilters[i].Tick(input + combCoeff[i] * combFilters[i].Last);

                for (int i = 0; i < 3; i++)
                {
                    var temp1 = temp0 + allPassCoeff * allPassFilters[i].Last;
                    temp0 = allPassFilters[i].Tick(temp1) - allPassCoeff * temp1;
                }

                lowpassState = 0.7f * lowpassState + 0.3f * temp0;
                var temp2 = lowpassState + allPassCoeff * allPassFilters[3].Last;
                temp2 = allPassFilters[3].Tick(temp2) - allPassCoeff * temp2;

                var out1 = temp2 + allPassCoeff * allPassFilters[4].Last;
                var out2 = temp2 + allPassCoeff * allPassFilters[5].Last;

                out1 = allPassFilters[4].Tick(out1) - allPassCoeff * out1;
                out2 = allPassFilters[5].Tick(out2) - allPassCoeff * out2;

                out1 = mix * out1 + (1.0f - mix) * data[offset];
                out2 = mix * out2 + (1.0f - mix) * data[offset + 1];

                data[offset] = out1;
                data[offset + 1] = out2;
            }

            return data;
        }
    }
}