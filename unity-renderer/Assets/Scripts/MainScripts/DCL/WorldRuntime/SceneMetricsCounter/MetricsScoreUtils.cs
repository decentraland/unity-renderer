using UnityEngine;

namespace DCL
{
    public static class MetricsScoreUtils
    {
        public static float ComputeAudioClipScore(AudioClip audioClip)
        {
            const float BYTES_PER_SAMPLE = 2; // We assume sample resolution of 65535
            return audioClip.samples * audioClip.channels * BYTES_PER_SAMPLE;
        }

        public static float ComputeTextureScore(Texture2D texture)
        {
            const float MIPMAP_FACTOR = 1.3f;
            const float BYTES_PER_PIXEL = 4;
            return texture.width * texture.height * BYTES_PER_PIXEL * MIPMAP_FACTOR;
        }
    }
}