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
            service = new StableDiffusionService(WebRequestController.Create(), JObject.Parse(jsonFile.text), null);
        }

        [ContextMenu("Generate")]
        private async void Generate()
        {
            service = new StableDiffusionService(WebRequestController.Create(), JObject.Parse(jsonFile.text), null);
            var result = await service.GetTexture(new TextToImageConfig());
            targetImage.texture = result;
        }
    }
}
