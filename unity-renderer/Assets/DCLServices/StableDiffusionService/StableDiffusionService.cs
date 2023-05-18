using Cysharp.Threading.Tasks;
using DCL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.StableDiffusionService
{
    public class StableDiffusionService : IStableDiffusionService
    {
        private const string URL_TXT2IMG = "http://127.0.0.1:7860/sdapi/v1/txt2img";
        private const string URL_IMG2IMG = "http://127.0.0.1:7860/sdapi/v1/img2img";

        private readonly WebRequestController webRequestController;
        private readonly JObject txt2ImgConfig;
        private readonly JObject img2ImgConfig;

        public StableDiffusionService(WebRequestController webRequestController, JObject txt2imgConfig, JObject img2imgConfig)
        {
            this.webRequestController = webRequestController;
            txt2ImgConfig = txt2imgConfig;
            img2ImgConfig = img2imgConfig;
        }

        public async UniTask<Texture2D> GetTexture(TextToImageConfig config)
        {
            var response = await webRequestController.PostAsync(URL_TXT2IMG, GenerateText2ImgPayload(config));

            byte[] imageBytes = Convert.FromBase64String(response.downloadHandler.text);

            // Create a new Texture2D and load the decoded image into it
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            tex.Apply(false, true);
            return tex;
        }

        public UniTask<Texture2D> GetTexture(Texture2D sourceImg, ImageToImageConfig config) =>
            throw new NotImplementedException();

        private string GenerateText2ImgPayload(TextToImageConfig config)
        {
            var clone = txt2ImgConfig.DeepClone();

            clone["cfg_scale"] = config.cfgScale;
            clone["width"] = config.width;
            clone["height"] = config.height;
            clone["seed"] = config.seed;
            clone["negative_prompt"] = config.negativePrompt;
            clone["prompt"] = config.prompt;
            clone["steps"] = config.samplingSteps;

            return JsonConvert.SerializeObject(clone, Formatting.Indented);
        }

        private string GenerateImg2ImgPayload(Texture2D text, ImageToImageConfig config)
        {
            var clone = txt2ImgConfig.DeepClone();

            clone["init_images"] = new JArray { Convert.ToBase64String(text.EncodeToPNG()) };
            clone["cfg_scale"] = config.cfgScale;
            clone["width"] = config.width;
            clone["height"] = config.height;
            clone["seed"] = config.seed;
            clone["negative_prompt"] = config.negativePrompt;
            clone["prompt"] = config.prompt;
            clone["steps"] = config.samplingSteps;
            clone["denoising_strength"] = config.denoisingStrength;

            return JsonConvert.SerializeObject(clone, Formatting.Indented);
        }


    }
}
