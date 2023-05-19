using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.StableDiffusionService
{
    public class StableDiffusionService : IStableDiffusionService
    {
        private const string URL_TXT2IMG = "http://127.0.0.1:7860/sdapi/v1/txt2img";
        private const string URL_IMG2IMG = "http://127.0.0.1:7860/sdapi/v1/img2img";

        private readonly JObject txt2ImgConfig;
        private readonly JObject img2ImgConfig;

        public StableDiffusionService(JObject txt2imgConfig, JObject img2imgConfig)
        {
            txt2ImgConfig = txt2imgConfig;
            img2ImgConfig = img2imgConfig;
        }

        public async UniTask<Texture2D> GetTexture(TextToImageConfig config)
        {
            string payload = GenerateText2ImgPayload(config);
            Texture2D tex = await RequestAndCreateTexture(payload, URL_TXT2IMG);
            return tex;
        }

        public async UniTask<Texture2D> GetTexture(Texture2D sourceImg, ImageToImageConfig config)
        {
            string payload = GenerateImg2ImgPayload(sourceImg, config);
            Texture2D tex = await RequestAndCreateTexture(payload, URL_IMG2IMG);
            return tex;
        }

        private static async Task<Texture2D> RequestAndCreateTexture(string payload, string url)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var response = await request.SendWebRequest();

            JObject jObject = JObject.Parse(response.downloadHandler.text);
            var base64Img = jObject["images"].Children().First().Value<string>();

            byte[] imageBytes = Convert.FromBase64String(base64Img);

            // Create a new Texture2D and load the decoded image into it
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            tex.Apply(false, true);
            return tex;
        }

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

        private string GenerateImg2ImgPayload(Texture2D img, ImageToImageConfig config)
        {
            var clone = img2ImgConfig.DeepClone();

            string base64String = Convert.ToBase64String(img.EncodeToPNG());

            clone["init_images"] = new JArray {
                base64String
            };
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

        public void Dispose()
        {

        }

        public void Initialize()
        {
        }
    }
}
