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

        private readonly List<WebRequestAsyncOperation> ongoingWebRequests = new ();

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

        public IWebRequestAsyncOperation Get(
            string url,
            DownloadHandler downloadHandler = null,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            Dictionary<string, string> headers = null)
        {
            return SendWebRequest(getWebRequestFactory, url, downloadHandler, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public IWebRequestAsyncOperation Post(
            string url,
            string postData,
            DownloadHandler downloadHandler = null,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            Dictionary<string, string> headers = null)
        {
            postWebRequestFactory.SetBody(postData);
            return SendWebRequest(postWebRequestFactory, url, downloadHandler, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public IWebRequestAsyncOperation GetAssetBundle(
            string url,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            return SendWebRequest(assetBundleFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public IWebRequestAsyncOperation GetAssetBundle(
            string url,
            Hash128 hash,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            assetBundleFactory.SetHash(hash);
            return SendWebRequest(assetBundleFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public IWebRequestAsyncOperation GetTexture(
            string url,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            bool isReadable = true,
            Dictionary<string, string> headers = null)
        {
            textureFactory.isReadable = isReadable;
            return SendWebRequest(textureFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public IWebRequestAsyncOperation GetAudioClip(
            string url,
            AudioType audioType,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            audioClipWebRequestFactory.SetAudioType(audioType);
            return SendWebRequest(audioClipWebRequestFactory, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        private WebRequestAsyncOperation SendWebRequest<T>(
            T requestFactory,
            string url,
            DownloadHandler downloadHandler,
            Action<IWebRequestAsyncOperation> OnSuccess,
            Action<IWebRequestAsyncOperation> OnFail,
            int requestAttemps,
            int timeout,
            bool disposeOnCompleted,
            Dictionary<string, string> headers = null,
            WebRequestAsyncOperation asyncOp = null
        ) where T: IWebRequestFactory
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = requestFactory.CreateWebRequest(url);
            request.timeout = timeout;

            if (headers != null)
            {
                foreach (var item in headers) { request.SetRequestHeader(item.Key, item.Value); }
            }

            if (downloadHandler != null)
                request.downloadHandler = downloadHandler;

            WebRequestAsyncOperation resultOp = asyncOp;

            if (resultOp == null)
                resultOp = new WebRequestAsyncOperation(request);
            else
                resultOp.SetNewWebRequest(request);

            resultOp.disposeOnCompleted = disposeOnCompleted;
            ongoingWebRequests.Add(resultOp);

            UnityWebRequestAsyncOperation requestOp = resultOp.SendWebRequest();

            requestOp.completed += (asyncOp) =>
            {
                if (!resultOp.isDisposed)
                {
                    if (resultOp.webRequest.WebRequestSucceded())
                    {
                        OnSuccess?.Invoke(resultOp);
                        resultOp.SetAsCompleted(true);
                    }
                    else if (!resultOp.webRequest.WebRequestAborted() && resultOp.webRequest.WebRequestServerError())
                    {
                        remainingAttemps--;

                        if (remainingAttemps > 0)
                        {
                            resultOp.Dispose();
                            resultOp = SendWebRequest(requestFactory, url, downloadHandler, OnSuccess, OnFail, remainingAttemps, timeout, disposeOnCompleted, headers, resultOp);
                        }
                        else
                        {
                            OnFail?.Invoke(resultOp);
                            resultOp.SetAsCompleted(false);
                        }
                    }
                    else
                    {
                        OnFail?.Invoke(resultOp);
                        resultOp.SetAsCompleted(false);
                    }
                }

                ongoingWebRequests.Remove(resultOp);
            };

            return resultOp;
        }

        public void Dispose()
        {
            foreach (var webRequest in ongoingWebRequests) { webRequest.Dispose(); }
        }
    }
}
