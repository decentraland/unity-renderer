using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

namespace DCLServices.StableDiffusionService
{
    public struct TextToImageConfig
    {
        public string prompt;
        public string negativePrompt;
        public int seed;
        public int width;
        public int height;
        public int samplingSteps;
        public float cfgScale;
    }

    public struct ImageToImageConfig
    {
        public string prompt;
        public string negativePrompt;
        public int seed;
        public int width;
        public int height;
        public int samplingSteps;
        public float cfgScale;
        public float denoisingStrength;
    }

    public interface IStableDiffusionService : IService
    {
        UniTask<Texture2D> GetTexture(TextToImageConfig config);

        UniTask<Texture2D> GetTexture(Texture2D sourceImg, ImageToImageConfig config);
    }
}
