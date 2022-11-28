using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;
using Altom.AltTester.Logging;
using Newtonsoft.Json;

namespace Altom.AltTester.Commands
{
    public abstract class AltBaseScreenshotCommand<TParams, TResult> : AltCommand<TParams, TResult> where TParams : CommandParams
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();
        protected readonly ICommandHandler Handler;

        protected AltBaseScreenshotCommand(ICommandHandler handler, TParams cmdParams) : base(cmdParams)
        {
            this.Handler = handler;
        }

        public abstract override TResult Execute();


        protected System.Collections.IEnumerator SendTexturedScreenshotCoroutine(UnityEngine.Vector2 size, int quality)
        {
            yield return new UnityEngine.WaitForEndOfFrame();
            sendTexturedScreenshotResponse(size, quality);
        }

        protected System.Collections.IEnumerator SendPNGScreenshotCoroutine()
        {
            yield return new UnityEngine.WaitForEndOfFrame();
            var response = ExecuteAndSerialize(getPNGScreenshot);
            Handler.Send(response);
        }

        protected System.Collections.IEnumerator SendScreenshotObjectHighlightedCoroutine(UnityEngine.Vector2 size, int quality, UnityEngine.GameObject gameObject, UnityEngine.Color color, float width)
        {
            UnityEngine.Renderer renderer = gameObject.GetComponent<UnityEngine.Renderer>();
            if (renderer != null)
            {
                var originalMaterials = renderer.materials.ToArray();
                renderer.materials = new UnityEngine.Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    renderer.materials[i] = new UnityEngine.Material(originalMaterials[i]);
                    renderer.materials[i].shader = AltRunner._altRunner.outlineShader;
                    renderer.materials[i].SetColor("_OutlineColor", color);
                    renderer.materials[i].SetFloat("_OutlineWidth", width);
                }
                yield return new UnityEngine.WaitForEndOfFrame();
                sendTexturedScreenshotResponse(size, quality);

                renderer.materials = originalMaterials;
            }
            else
            {
                var rectTransform = gameObject.GetComponent<UnityEngine.RectTransform>();
                if (rectTransform != null)
                {
                    var panelHighlight = UnityEngine.Object.Instantiate(AltRunner._altRunner.panelHightlightPrefab, rectTransform);
                    panelHighlight.GetComponent<UnityEngine.UI.Image>().color = color;

                    yield return new UnityEngine.WaitForEndOfFrame();
                    sendTexturedScreenshotResponse(size, quality);

                    UnityEngine.Object.Destroy(panelHighlight);
                }
                else
                {
                    yield return new UnityEngine.WaitForEndOfFrame();
                    sendTexturedScreenshotResponse(size, quality);
                }
            }
        }

        private void sendTexturedScreenshotResponse(UnityEngine.Vector2 size, int quality)
        {
            var result = ExecuteHandleErrors(() => getTexturedScreenshot(size, quality));

            string data = JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = CultureInfo.InvariantCulture,
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            });

            Handler.Send(data);

        }

        private AltGetScreenshotResponse getTexturedScreenshot(UnityEngine.Vector2 size, int quality)
        {
            var screenshot = UnityEngine.ScreenCapture.CaptureScreenshotAsTexture();
            int width = (int)size.x;
            int height = (int)size.y;

            if (width == 0 && height == 0)
            {
                width = screenshot.width;
                height = screenshot.height;
            }
            else
            {
                var heightDifference = screenshot.height - height;
                var widthDifference = screenshot.width - width;

                if (heightDifference > widthDifference)
                {
                    width = height * screenshot.width / screenshot.height;
                }
                else
                {
                    height = width * screenshot.height / screenshot.width;
                }
            }

            AltGetScreenshotResponse response = new AltGetScreenshotResponse();

            response.scaleDifference = new AltVector2(screenshot.width, screenshot.height);
            quality = UnityEngine.Mathf.Clamp(quality, 1, 100);
            if (quality != 100)
            {
                width = width * quality / 100;
                height = height * quality / 100;
                AltTextureScale.Bilinear(screenshot, width, height);
            }
            var screenshotSerialized = UnityEngine.ImageConversion.EncodeToPNG(screenshot);

            logger.Trace("Start Compression");
            var screenshotCompressed = AltRunner.CompressScreenshot(screenshotSerialized);
            logger.Trace("Finished Compression");

            response.textureSize = new AltVector3(screenshot.width, screenshot.height);
            response.compressedImage = screenshotCompressed;

            UnityEngine.Object.DestroyImmediate(screenshot);
            return response;
        }

        private string getPNGScreenshot()
        {

            var screenShot = new UnityEngine.Texture2D(UnityEngine.Screen.width, UnityEngine.Screen.height, UnityEngine.TextureFormat.RGB24, false);
            screenShot.ReadPixels(new UnityEngine.Rect(0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height), 0, 0);
            screenShot.Apply();
            var bytesPNG = UnityEngine.ImageConversion.EncodeToPNG(screenShot);
            var pngAsString = Convert.ToBase64String(bytesPNG);
            UnityEngine.Object.DestroyImmediate(screenShot);
            return pngAsString;
        }
    }
}
