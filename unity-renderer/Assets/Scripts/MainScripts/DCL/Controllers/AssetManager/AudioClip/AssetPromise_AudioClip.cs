using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace DCL
{
    public class AssetPromise_AudioClip : AssetPromise<Asset_AudioClip>
    {

        struct QueueItem
        {
            public AssetPromise_AudioClip promise;
            public UnityWebRequest request;
        }

        private readonly string url;
        private readonly string id;
        private readonly AudioType audioType;
        private readonly IWebRequestController webRequestController;
        private IWebRequestAsyncOperation webRequestAsyncOperation;
        private Action OnSuccess;
        private Coroutine coroutineQueueLoader;
        private static Queue<QueueItem> requestQueue = new Queue<QueueItem>();
        private static readonly RendererState renderState = CommonScriptableObjects.rendererState;
        private QueueItem queueItem;
        public AssetPromise_AudioClip(string clipPath, ContentProvider provider) : this(clipPath, provider, Environment.i.platform.webRequest) { }

        public AssetPromise_AudioClip(string clipPath, ContentProvider provider, IWebRequestController webRequestController)
        {
            this.url = provider.GetContentsUrl(clipPath);
            this.id = this.url != null ? this.url.Substring(this.url.LastIndexOf('/') + 1) : $"{GetHashCode()}";
            this.audioType = GetAudioTypeFromUrlName(clipPath);
            this.webRequestController = webRequestController;
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            this.OnSuccess = OnSuccess;

            if (string.IsNullOrEmpty(url))
            {
                OnFail?.Invoke(new Exception("Audio clip url is null or empty"));
                return;
            }
            webRequestAsyncOperation = webRequestController.GetAudioClip(url, audioType,
                request =>
                {
                    if (renderState.Get())
                    {
                        queueItem = new QueueItem { promise = this, request = request.webRequest};
                        requestQueue.Enqueue(queueItem);
                        coroutineQueueLoader = CoroutineStarter.Start(CoroutineQueueLoader());
                    }
                    else
                    {
                        GetAudioClipFromRequest(this, request.webRequest);
                    }
                },
                request =>
                {
                    OnFail?.Invoke(new Exception($"Audio clip failed to fetch: {request?.webRequest?.error}"));
                }, disposeOnCompleted: false);
        }

        /// <summary>
        /// This coroutine prevents multiple audio clips being loaded at the same time, reducing hiccups in the process
        /// </summary>
        private IEnumerator CoroutineQueueLoader()
        {
            while (!requestQueue.Peek().Equals(queueItem))
            {
                yield return new WaitForEndOfFrame();
            }

            var request = requestQueue.Dequeue();
            GetAudioClipFromRequest(request.promise, request.request);
        }

        static void GetAudioClipFromRequest(AssetPromise_AudioClip promise, UnityWebRequest www)
        {
            ulong wwwDownloadedBytes = www.downloadedBytes;

            // files bigger than 1MB will be treated as streaming
            if (wwwDownloadedBytes > 1000000)
            {
                ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
            }

            promise.asset.audioClip = DownloadHandlerAudioClip.GetContent(www);
            promise.OnSuccess?.Invoke();
            promise.webRequestAsyncOperation?.Dispose();
        }

        protected override void OnCancelLoading()
        {
            requestQueue = new Queue<QueueItem>(requestQueue.Where(i => i.promise != this));
            CoroutineStarter.Stop(coroutineQueueLoader);
            webRequestAsyncOperation?.Dispose();
            webRequestAsyncOperation = null;
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                return false;
            }
            asset = library.Get(asset.id);
            return true;
        }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }

        public override object GetId()
        {
            return id;
        }

        private static AudioType GetAudioTypeFromUrlName(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("GetAudioTypeFromUrlName >>> Null url!");
                return AudioType.UNKNOWN;
            }

            string ext = url.Substring(url.Length - 3).ToLower();

            switch (ext)
            {
                case "mp3":
                    return AudioType.MPEG;
                case "wav":
                    return AudioType.WAV;
                case "ogg":
                    return AudioType.OGGVORBIS;
                default:
                    return AudioType.UNKNOWN;
            }
        }
    }
}
