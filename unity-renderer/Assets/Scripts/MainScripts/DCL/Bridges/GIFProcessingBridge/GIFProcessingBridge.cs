using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DCL.Helpers;

namespace DCL
{
    /// <summary>
    /// Bridge that handles GIF processing requests for kernel (initiated at Asset_Gif.Load()), and relays the kernel converted textures back to the Asset_Gif
    /// </summary>
    public class GIFProcessingBridge : MonoBehaviour
    {
        [System.Serializable]
        public class UpdateGIFPointersPayload
        {
            public string id;
            public int width;
            public int height;
            public int[] pointers;
            public float[] frameDelays;
        }

        public class PendingGifs
        {
            public enum Status { PENDING, OK, ERROR }

            public GifFrameData[] textures;
            public Status status;
        }

        public static GIFProcessingBridge i { get; private set; }

        private readonly Dictionary<string, PendingGifs> pendingGifs = new Dictionary<string, PendingGifs>();

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

            KernelConfig.i.EnsureConfigInitialized().Then(config => { jsGIFProcessingEnabled = config.gifSupported; });
        }

        bool jsGIFProcessingEnabled = false;

        /// <summary>
        /// Tells Kernel to start processing a desired GIF, waits for the data to come back from Kernel and passes it to the GIF through the onFinishCallback
        /// </summary>
        /// <param name="onSuccess">The callback that will be invoked with the generated textures list</param>
        public IEnumerator RequestGIFProcessor(string url, System.Action<GifFrameData[]> onSuccess, System.Action onFail)
        {
            if (!jsGIFProcessingEnabled)
            {
                onFail?.Invoke();
                yield break;
            }

            if (!pendingGifs.TryGetValue(url, out PendingGifs gif))
            {
                gif = new PendingGifs()
                {
                    status = PendingGifs.Status.PENDING
                };
                pendingGifs.Add(url, gif);
                DCL.Interface.WebInterface.RequestGIFProcessor(
                       url, url, SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
            }

            yield return new WaitUntil(() => gif.status != PendingGifs.Status.PENDING);

            if (gif.status == PendingGifs.Status.ERROR)
            {
                onFail?.Invoke();
                RemovePending(url);
                yield break;
            }

            onSuccess?.Invoke(gif.textures);
            RemovePending(url);
        }

        /// <summary>
        /// Method called by Kernel when a GIF finishes processing on that side. This method populates the pending GIF data that will unlock the waiting RequestGIFProcessor coroutine
        /// </summary>
        public void UpdateGIFPointers(string payload)
        {
            var parsedPayload = Utils.SafeFromJson<UpdateGIFPointersPayload>(payload);

            if (pendingGifs.TryGetValue(parsedPayload.id, out PendingGifs gif))
            {
                gif.textures = GenerateTexturesList(parsedPayload.width, parsedPayload.height, parsedPayload.pointers, parsedPayload.frameDelays);
                gif.status = PendingGifs.Status.OK;
            }
        }

        public void FailGIFFetch(string id)
        {
            RemovePending(id);
        }

        public void DeleteGIF(string id)
        {
            DCL.Interface.WebInterface.DeleteGIF(id);
        }

        private GifFrameData[] GenerateTexturesList(int width, int height, int[] pointers, float[] frameDelays)
        {
            if (width == 0 || height == 0)
            {
                Debug.Log("Couldn't create external textures! width or height are 0!");
                return null;
            }

            GifFrameData[] gifTextures = new GifFrameData[pointers.Length];
            for (int j = 0; j < pointers.Length; j++)
            {
                Texture2D newTex = Texture2D.CreateExternalTexture(width, height, TextureFormat.ARGB32,
                    false, false, (System.IntPtr)pointers[j]);

                if (newTex == null)
                {
                    Debug.Log("Couldn't create external texture!");
                    continue;
                }

                newTex.wrapMode = TextureWrapMode.Clamp;

                gifTextures[j] = new GifFrameData() {texture = newTex, delay = frameDelays[j] / 1000};
            }

            return gifTextures;
        }

        private void RemovePending(string id)
        {
            if (pendingGifs.TryGetValue(id, out PendingGifs gif))
            {
                gif.status = PendingGifs.Status.ERROR;
                pendingGifs.Remove(id);
            }
        }
    }
}