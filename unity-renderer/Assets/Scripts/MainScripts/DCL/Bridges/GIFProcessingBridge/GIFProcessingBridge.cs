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

        // TODO: Change into a struct???
        class GIFDataContainer
        {
            public UpdateGIFPointersPayload data = null;
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
        }

        bool jsGIFProcessingEnabled = true;
        Dictionary<string, GIFDataContainer> pendingGIFs = new Dictionary<string, GIFDataContainer>();

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

            var gifDataContainer = new GIFDataContainer();
            string pendingGifId = url;

            // If we are already waiting for a GIF with the same URL, make another unique id, to avoid sharing the same textures and have problems when disposing them
            while (pendingGIFs.ContainsKey(pendingGifId))
            {
                pendingGifId = "1" + pendingGifId;
            }

            pendingGIFs.Add(pendingGifId, gifDataContainer);

            DCL.Interface.WebInterface.RequestGIFProcessor(url, pendingGifId, SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);

            // We use a container class instead of just UpdateGIFPointersPayload to hold its reference and avoid accessing the collection on every yield check
            yield return new WaitUntil(() => !jsGIFProcessingEnabled || gifDataContainer.data != null);

            if (jsGIFProcessingEnabled)
                onSuccess?.Invoke(GenerateTexturesList(gifDataContainer.data.width, gifDataContainer.data.height, gifDataContainer.data.pointers, gifDataContainer.data.frameDelays));
            else
                onFail?.Invoke();

            pendingGIFs.Remove(pendingGifId);
        }

        /// <summary>
        /// Method called by Kernel when a GIF finishes processing on that side. This method populates the pending GIF data that will unlock the waiting RequestGIFProcessor coroutine
        /// </summary>
        public void UpdateGIFPointers(string payload)
        {
            var parsedPayload = Utils.SafeFromJson<UpdateGIFPointersPayload>(payload);

            if (pendingGIFs.ContainsKey(parsedPayload.id))
                pendingGIFs[parsedPayload.id].data = parsedPayload;
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
