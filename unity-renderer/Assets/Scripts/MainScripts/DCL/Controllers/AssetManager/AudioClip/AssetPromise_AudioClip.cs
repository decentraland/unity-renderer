using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AudioClip : AssetPromise<Asset_AudioClip>
    {
        private readonly string url;
        private readonly string id;
        private readonly AudioType audioType;
        private readonly IWebRequestController webRequestController;

        private WebRequestAsyncOperation webRequestAsyncOperation;

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
            if (string.IsNullOrEmpty(url))
            {
                OnFail?.Invoke(new Exception("Audio clip url is null or empty"));
                return;
            }

            webRequestAsyncOperation = webRequestController.GetAudioClip(url, audioType,
                request =>
                {
                    asset.audioClip = DownloadHandlerAudioClip.GetContent(request.webRequest);
                    OnSuccess?.Invoke();
                },
                request =>
                {
                    OnFail?.Invoke(new Exception($"Audio clip failed to fetch: {request?.webRequest?.error}"));
                });
        }

        protected override void OnCancelLoading()
        {
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