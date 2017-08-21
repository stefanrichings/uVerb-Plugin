using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uVerb
{
    public class DelayBuffer
    {
        float[] buffer;
        int position;

        public int Length
        {
            get { return buffer.Length; }
        }

        public float Last
        {
            get { return buffer[position]; }
        }

        public DelayBuffer (int length)
        {
            buffer = new float[length];
        }

        public float Tick (float input)
        {
            var last = buffer[position];
            buffer[position] = input;
            if (++position >= buffer.Length) position = 0;
            return last;
        }
    }
}
