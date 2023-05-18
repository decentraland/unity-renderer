using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCLServices.StableDiffusionService
{
    public struct TextToImageConfig
    {
        public string prompt;
        public string negativePrompt;
        public string seed;
        public int width;
        public int height;
        public int samplingSteps;
        public float cfgScale;
    }

    public struct ImageToImageConfig
    {
        public string prompt;
        public string negativePrompt;
        public string seed;
        public int width;
        public int height;
        public float cfgScale;
        public int samplingSteps;
        public float denoisingStrength;
    }

    public interface IStableDiffusionService
    {
        UniTask<Texture2D> GetTexture(TextToImageConfig config);

        UniTask<Texture2D> GetTexture(Texture2D sourceImg, ImageToImageConfig config);
    }
}
