using UnityEngine;

namespace DCL
{
    public static class MetricsScoreUtils
    {
        public static long ComputeAudioClipScore(AudioClip audioClip)
        {
            const double BYTES_PER_SAMPLE = 2; // We assume sample resolution of 65535
            return (long) (audioClip.samples * audioClip.channels * BYTES_PER_SAMPLE);
        }

        public static long ComputeTextureScore(Texture2D texture)
        {
            const double MIPMAP_FACTOR = 2.4f;
            const double BYTES_PER_PIXEL = 4;
            return (long) (texture.width * texture.height * BYTES_PER_PIXEL * MIPMAP_FACTOR);
        }
    }
}