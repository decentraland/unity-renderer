using DCL;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLServices.StableDiffusionService
{
    public class StableDiffusionTest : MonoBehaviour
    {
        [SerializeField] private TextAsset jsonFile;
        [SerializeField] private RawImage targetImage;
        private StableDiffusionService service;

        private void Start()
        {
            service = new StableDiffusionService(JObject.Parse(jsonFile.text), null);
        }

        [ContextMenu("Generate")]
        private async void Generate()
        {
            service = new StableDiffusionService(JObject.Parse(jsonFile.text), null);

            var result = await service.GetTexture(new TextToImageConfig()
                {
                    cfgScale = 4,
                    height = 512,
                    width = 512,
                    samplingSteps = 20,
                    negativePrompt = "",
                    prompt = "A beautiful landscape",
                    seed = -1,
                }
            );

            targetImage.texture = result;
        }
    }
}
