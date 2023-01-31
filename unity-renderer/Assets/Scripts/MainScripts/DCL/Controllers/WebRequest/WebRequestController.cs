using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
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
            getWebRequestFactory = genericWebRequest;
            assetBundleFactory = assetBundleFactoryWebRequest;
            textureFactory = textureFactoryWebRequest;
            audioClipWebRequestFactory = audioClipWebRequest;
        }

        public async UniTask<UnityWebRequest> Get(
            string url,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null)
        {
            return await SendWebRequest(getWebRequestFactory, url, downloadHandler, onSuccess, onfail, requestAttemps,
                timeout, disposeOnCompleted, cancellationToken, headers);
        }

        public async UniTask<UnityWebRequest> Post(
            string url,
            string postData,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null)
        {
            postWebRequestFactory.SetBody(postData);
            return await SendWebRequest(postWebRequestFactory, url, downloadHandler, onSuccess, onfail, requestAttemps,
                timeout, disposeOnCompleted, cancellationToken, headers);
        }

        public async UniTask<UnityWebRequest> GetAssetBundle(
            string url,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            CancellationToken cancellationToken = default)
        {
            return await SendWebRequest(assetBundleFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, disposeOnCompleted, cancellationToken);
        }

        public async UniTask<UnityWebRequest> GetAssetBundle(
            string url,
            Hash128 hash,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            CancellationToken cancellationToken = default)
        {
            assetBundleFactory.SetHash(hash);
            return await SendWebRequest(assetBundleFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, disposeOnCompleted, cancellationToken);
        }

        public async UniTask<UnityWebRequest> GetTexture(
            string url,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            bool isReadable = true,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null)
        {
            textureFactory.isReadable = isReadable;
            return await SendWebRequest(textureFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, disposeOnCompleted, cancellationToken, headers);
        }

        public async UniTask<UnityWebRequest> GetAudioClip(
            string url,
            AudioType audioType,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            CancellationToken cancellationToken = default)
        {
            audioClipWebRequestFactory.SetAudioType(audioType);
            return await SendWebRequest(audioClipWebRequestFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, disposeOnCompleted, cancellationToken);
        }

        private async UniTask<UnityWebRequest> SendWebRequest<T>(
            T requestFactory,
            string url,
            DownloadHandler downloadHandler,
            Action<UnityWebRequest> onSuccess,
            Action<UnityWebRequest> onFail,
            int requestAttemps,
            int timeout,
            bool disposeOnCompleted,
            CancellationToken cancellationToken,
            Dictionary<string, string> headers = null) where T : IWebRequestFactory
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            for (int i = 0; i < requestAttemps; i++)
            {
                UnityWebRequest request = requestFactory.CreateWebRequest(url);
                request.timeout = timeout;

                if (headers != null)
                {
                    foreach (var item in headers)
                        request.SetRequestHeader(item.Key, item.Value);
                }

                if (downloadHandler != null)
                    request.downloadHandler = downloadHandler;

                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await UniTask.Yield();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        request.Abort();
                        break;
                    }
                }

                if (asyncOp.webRequest == null)
                    continue;

                if (request.WebRequestSucceded())
                {
                    onSuccess?.Invoke(asyncOp.webRequest);
                    DisposeRequestIfNeeded(request, disposeOnCompleted);
                    return asyncOp.webRequest;
                }
                else if (!request.WebRequestAborted() && request.WebRequestServerError())
                {
                    remainingAttemps--;

                    if (remainingAttemps > 0)
                    {
                        DisposeRequestIfNeeded(request, true);
                        continue;
                    }
                    else
                    {
                        onFail?.Invoke(asyncOp.webRequest);
                        DisposeRequestIfNeeded(request, disposeOnCompleted);
                        return asyncOp.webRequest;
                    }
                }
                else
                {
                    onFail?.Invoke(asyncOp.webRequest);
                    DisposeRequestIfNeeded(request, disposeOnCompleted);
                    return asyncOp.webRequest;
                }
            }

            return null;
        }

        public void Dispose() { }


        private void DisposeRequestIfNeeded(UnityWebRequest request, bool shouldBeDisposed)
        {
            if (!shouldBeDisposed)
                return;

            request.Dispose();
        }
    }
}
