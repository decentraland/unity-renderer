using UnityEngine;

namespace DCL
{
    public static class MetricsScoreUtils
    {
        public static long ComputeAudioClipScore(AudioClip audioClip)
        {
            long baseOverhead = 3000; // Measured 2708 profiling AudioClip with 1 sample and 2 channels.
            // Rounding up just in case.

            // For most cases the sample resolution should be of 65535 (PCM)
            const double BYTES_PER_SAMPLE = 2;
            return (long) (audioClip.samples * audioClip.channels * BYTES_PER_SAMPLE) + baseOverhead;
        }

        public static long ComputeTextureScore(Texture2D texture)
        {
            // https://forum.unity.com/threads/does-unity-always-use-double-memory-for-texture-in-runtime.198270/
            const double ARBITRARY_UNITY_MEMORY_MULTIPLIER = 1;

            // The mipmap memory increase should be actually 1.33 according to many sources
            const double MIPMAP_FACTOR = 1.4f; 
            const double BYTES_PER_PIXEL = 4;
            return (long) (texture.width * texture.height * BYTES_PER_PIXEL * MIPMAP_FACTOR *
                           ARBITRARY_UNITY_MEMORY_MULTIPLIER);
        }
    }
}