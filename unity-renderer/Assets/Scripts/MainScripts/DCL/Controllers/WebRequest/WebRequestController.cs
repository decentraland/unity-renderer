using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class WebRequestController : IWebRequestController
    {
        private IWebRequestFactory getWebRequestFactory;
        private IWebRequestAssetBundleFactory assetBundleFactory;
        private IWebRequestTextureFactory textureFactory;
        private IWebRequestAudioFactory audioClipWebRequestFactory;
        private IPostWebRequestFactory postWebRequestFactory;

        private WebRequestController(
            IWebRequestFactory getWebRequestFactory,
            IWebRequestAssetBundleFactory assetBundleFactory,
            IWebRequestTextureFactory textureFactory,
            IWebRequestAudioFactory audioClipWebRequestFactory,
            IPostWebRequestFactory postWebRequestFactory
        )
        {
            this.getWebRequestFactory = getWebRequestFactory;
            this.assetBundleFactory = assetBundleFactory;
            this.textureFactory = textureFactory;
            this.audioClipWebRequestFactory = audioClipWebRequestFactory;
            this.postWebRequestFactory = postWebRequestFactory;
        }

        public void Initialize() { }

        public static WebRequestController Create()
        {
            WebRequestController newWebRequestController = new WebRequestController(
                new GetWebRequestFactory(),
                new WebRequestAssetBundleFactory(),
                new WebRequestTextureFactory(),
                new WebRequestAudioFactory(),
                new PostWebRequestFactory()
            );

            return newWebRequestController;
        }

        public void Initialize(
            IWebRequestFactory genericWebRequest,
            IWebRequestAssetBundleFactory assetBundleFactoryWebRequest,
            IWebRequestTextureFactory textureFactoryWebRequest,
            IWebRequestAudioFactory audioClipWebRequest)
        {
            this.getWebRequestFactory = genericWebRequest;
            this.assetBundleFactory = assetBundleFactoryWebRequest;
            this.textureFactory = textureFactoryWebRequest;
            this.audioClipWebRequestFactory = audioClipWebRequest;
        }

        public UnityWebRequestAsyncOperation Get(
            string url,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequestAsyncOperation> OnSuccess = null,
            Action<UnityWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            Dictionary<string, string> headers = null)
        {
            return SendWebRequest(getWebRequestFactory, url, downloadHandler, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public UnityWebRequestAsyncOperation Post(
            string url,
            string postData,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequestAsyncOperation> OnSuccess = null,
            Action<UnityWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            Dictionary<string, string> headers = null)
        {
            postWebRequestFactory.SetBody(postData);
            return SendWebRequest(postWebRequestFactory, url, downloadHandler, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public UnityWebRequestAsyncOperation GetAssetBundle(
            string url,
            Action<UnityWebRequestAsyncOperation> OnSuccess = null,
            Action<UnityWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            return SendWebRequest(assetBundleFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public UnityWebRequestAsyncOperation GetAssetBundle(
            string url,
            Hash128 hash,
            Action<UnityWebRequestAsyncOperation> OnSuccess = null,
            Action<UnityWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            assetBundleFactory.SetHash(hash);
            return SendWebRequest(assetBundleFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public UnityWebRequestAsyncOperation GetTexture(
            string url,
            Action<UnityWebRequestAsyncOperation> OnSuccess = null,
            Action<UnityWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            bool isReadable = true,
            Dictionary<string, string> headers = null)
        {
            textureFactory.isReadable = isReadable;
            return SendWebRequest(textureFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public UnityWebRequestAsyncOperation GetAudioClip(
            string url,
            AudioType audioType,
            Action<UnityWebRequestAsyncOperation> OnSuccess = null,
            Action<UnityWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            audioClipWebRequestFactory.SetAudioType(audioType);
            return SendWebRequest(audioClipWebRequestFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        private UnityWebRequestAsyncOperation SendWebRequest<T>(
            T requestFactory,
            string url,
            DownloadHandler downloadHandler,
            Action<UnityWebRequestAsyncOperation> OnSuccess,
            Action<UnityWebRequestAsyncOperation> OnFail,
            int requestAttemps,
            int timeout,
            bool disposeOnCompleted,
            Dictionary<string, string> headers = null
        ) where T: IWebRequestFactory
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = requestFactory.CreateWebRequest(url);
            request.timeout = timeout;

            if (headers != null)
            {
                foreach (var item in headers)
                    request.SetRequestHeader(item.Key, item.Value);
            }

            if (downloadHandler != null)
                request.downloadHandler = downloadHandler;

            UnityWebRequestAsyncOperation requestOp = request.SendWebRequest();

            requestOp.completed += (asyncOp) =>
            {
                if (request != null)
                {
                    if (request.WebRequestSucceded())
                    {
                        OnSuccess?.Invoke(requestOp);
                        DisposeRequestIfNeeded(ref request, disposeOnCompleted);
                    }
                    else if (!request.WebRequestAborted() && request.WebRequestServerError())
                    {
                        remainingAttemps--;

                        if (remainingAttemps > 0)
                        {
                            DisposeRequestIfNeeded(ref request, true);
                            requestOp = SendWebRequest(requestFactory, url, downloadHandler, OnSuccess, OnFail, remainingAttemps, timeout, disposeOnCompleted, headers);
                        }
                        else
                        {
                            OnFail?.Invoke(requestOp);
                            DisposeRequestIfNeeded(ref request, disposeOnCompleted);
                        }
                    }
                    else
                    {
                        OnFail?.Invoke(requestOp);
                        DisposeRequestIfNeeded(ref request, disposeOnCompleted);
                    }
                }
            };

            return requestOp;
        }

        public void Dispose() { }
        

        private void DisposeRequestIfNeeded(ref UnityWebRequest request, bool shouldBeDisposed)
        {
            if (!shouldBeDisposed)
                return;
            
            request.Dispose();
            request = null;
        }
    }
}
