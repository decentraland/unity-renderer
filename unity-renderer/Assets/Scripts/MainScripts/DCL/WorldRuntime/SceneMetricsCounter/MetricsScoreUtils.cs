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
            // But the size seems to be rounded up to 4 bytes. (Maybe due to Mono aligning the bytes?)
            const double BYTES_PER_SAMPLE = 4;
            return (long) (audioClip.samples * audioClip.channels * BYTES_PER_SAMPLE) + baseOverhead;
        }

        public static long ComputeTextureScore(Texture2D texture)
        {
            // The mipmap memory increase should be actually 1.33 according to many sources
            // But for Unity it seems to be up to 2.33. This was tested even with GPU only textures.
            const double MIPMAP_FACTOR = 2.4f; 
            const double BYTES_PER_PIXEL = 4;
            return (long) (texture.width * texture.height * BYTES_PER_PIXEL * MIPMAP_FACTOR);
        }
    }
}