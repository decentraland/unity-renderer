using DCL;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLServices.StableDiffusionService
{
    public class StableDiffusionTest : MonoBehaviour
    {
        [SerializeField] private TextAsset txt2img;
        [SerializeField] private TextAsset img2img;
        [SerializeField] private RawImage targetImage;
        [SerializeField] private string positivePrompt = "A beautiful landscape, two suns";
        [SerializeField] private string negativePrompt = "humans, animals";
        [SerializeField] private Texture2D texturePrompt;
        [SerializeField] private float denoise = 0.5f;
        [SerializeField] private float scale = 7;
        private StableDiffusionService service;

        [ContextMenu("Generate From Text")]
        private async void Generate()
        {
            service = new StableDiffusionService(JObject.Parse(txt2img.text), JObject.Parse(img2img.text));

            var result = await service.GetTexture(GetTextToImageConfig());

            targetImage.texture = result;
        }

        [ContextMenu("Generate From Image")]
        private async void GenerateFromImage()
        {
            service = new StableDiffusionService(JObject.Parse(txt2img.text), JObject.Parse(img2img.text));

            var result = await service.GetTexture(texturePrompt, GetImageToImageConfig());

            targetImage.texture = result;
        }

        private TextToImageConfig GetTextToImageConfig() =>
            new ()
            {
                cfgScale = scale,
                height = 512,
                width = 512,
                samplingSteps = 20,
                negativePrompt = negativePrompt,
                prompt = positivePrompt,
                seed = -1,
            };

        private ImageToImageConfig GetImageToImageConfig() =>
            new ()
            {
                cfgScale = scale,
                height = 512,
                width = 512,
                samplingSteps = 40,
                negativePrompt = negativePrompt,
                prompt = positivePrompt,
                seed = -1,
                denoisingStrength = denoise,
            };
    }
}
