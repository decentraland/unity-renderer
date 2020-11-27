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

        public static GIFProcessingBridge i { get; private set; }

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
        Dictionary<string, GIFCache> cachedGIFs = new Dictionary<string, GIFCache>();

        /// <summary>
        /// Tells Kernel to start processing a desired GIF, waits for the data to come back from Kernel and passes it to the GIF through the onFinishCallback
        /// </summary>
        /// <param name="onSuccess">The callback that will be invoked with the generated textures list</param>
        public IEnumerator RequestGIFProcessor(string url, System.Action<List<UniGif.GifTexture>> onSuccess, System.Action onFail)
        {
            if (!jsGIFProcessingEnabled)
            {
                onFail?.Invoke();
                yield break;
            }

            string gifId = url;

            if (!cachedGIFs.TryGetValue(gifId, out GIFCache gif))
            {
                gif = new GIFCache()
                {
                    refCount = 0,
                    status = GIFCache.Status.PENDING,
                    data = new GIFCacheData()
                    {
                        id = gifId,
                        url = url,
                        textures = null
                    }
                };
                cachedGIFs.Add(gifId, gif);
                DCL.Interface.WebInterface.RequestGIFProcessor(
                       gif.data.url,
                       gif.data.id,
                       SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);
            }

            yield return new WaitUntil(() => gif.status != GIFCache.Status.PENDING);

            if (gif.status == GIFCache.Status.ERROR)
            {
                onFail?.Invoke();
                yield break;
            }

            gif.refCount++;
            onSuccess?.Invoke(gif.data.textures);
        }

        /// <summary>
        /// Method called by Kernel when a GIF finishes processing on that side. This method populates the pending GIF data that will unlock the waiting RequestGIFProcessor coroutine
        /// </summary>
        public void UpdateGIFPointers(string payload)
        {
            var parsedPayload = Utils.SafeFromJson<UpdateGIFPointersPayload>(payload);

            if (cachedGIFs.TryGetValue(parsedPayload.id, out GIFCache gif))
            {
                gif.data.textures = GenerateTexturesList(parsedPayload.width, parsedPayload.height, parsedPayload.pointers, parsedPayload.frameDelays);
                gif.status = GIFCache.Status.OK;
            }
        }

        public void FailGIFFetch(string id)
        {
            if (cachedGIFs.TryGetValue(id, out GIFCache gif))
            {
                gif.status = GIFCache.Status.ERROR;
                cachedGIFs.Remove(id);
            }
        }

        public void DeleteGIF(string id)
        {
            if (cachedGIFs.TryGetValue(id, out GIFCache gif))
            {
                gif.refCount = Mathf.Max(0, gif.refCount - 1);
                if (gif.refCount == 0)
                {
                    gif.status = GIFCache.Status.ERROR;
                    gif.Dispose();
                    cachedGIFs.Remove(id);
                    DCL.Interface.WebInterface.DeleteGIF(id);
                }
            }
        }

        public List<UniGif.GifTexture> GenerateTexturesList(int width, int height, int[] pointers, float[] frameDelays)
        {
            if (width == 0 || height == 0)
            {
                Debug.Log("Couldn't create external textures! width or height are 0!");
                return null;
            }

            List<UniGif.GifTexture> gifTexturesList = new List<UniGif.GifTexture>();
            for (int i = 0; i < pointers.Length; i++)
            {
                Texture2D newTex = Texture2D.CreateExternalTexture(width, height, TextureFormat.ARGB32, false, false, (System.IntPtr)pointers[i]);

                if (newTex == null)
                {
                    Debug.Log("Couldn't create external texture!");
                    continue;
                }

                newTex.wrapMode = TextureWrapMode.Clamp;

                gifTexturesList.Add(new UniGif.GifTexture(newTex, frameDelays[i] / 1000));
            }

            return gifTexturesList;
        }

        public void RejectGIFProcessingRequest()
        {
            jsGIFProcessingEnabled = false;
        }
    }
}