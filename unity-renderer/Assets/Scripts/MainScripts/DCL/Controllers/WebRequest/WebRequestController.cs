using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class WebRequestController : IWebRequestController
    {
        private const string TIMESTAMP_HEADER = "x-identity-timestamp";
        private const string METADATA_HEADER = "x-identity-metadata";

        private IWebRequestFactory getWebRequestFactory;
        private IWebRequestAssetBundleFactory assetBundleFactory;
        private IWebRequestTextureFactory textureFactory;
        private IWebRequestAudioFactory audioClipWebRequestFactory;
        private IPostWebRequestFactory postWebRequestFactory;
        private IPutWebRequestFactory putWebRequestFactory;
        private IPatchWebRequestFactory patchWebRequestFactory;
        private IDeleteWebRequestFactory deleteWebRequestFactory;
        private readonly IRPCSignRequest rpcSignRequest;

        private readonly List<WebRequestAsyncOperation> ongoingWebRequests = new();

        public WebRequestController(
            IWebRequestFactory getWebRequestFactory,
            IWebRequestAssetBundleFactory assetBundleFactory,
            IWebRequestTextureFactory textureFactory,
            IWebRequestAudioFactory audioClipWebRequestFactory,
            IPostWebRequestFactory postWebRequestFactory,
            IPutWebRequestFactory putWebRequestFactory,
            IPatchWebRequestFactory patchWebRequestFactory,
            IDeleteWebRequestFactory deleteWebRequestFactory,
            IRPCSignRequest rpcSignRequest = null
        )
        {
            this.getWebRequestFactory = getWebRequestFactory;
            this.assetBundleFactory = assetBundleFactory;
            this.textureFactory = textureFactory;
            this.audioClipWebRequestFactory = audioClipWebRequestFactory;
            this.postWebRequestFactory = postWebRequestFactory;
            this.putWebRequestFactory = putWebRequestFactory;
            this.patchWebRequestFactory = patchWebRequestFactory;
            this.deleteWebRequestFactory = deleteWebRequestFactory;
            this.rpcSignRequest = rpcSignRequest;
        }

        public void Initialize() { }

        public static WebRequestController Create()
        {
            WebRequestController newWebRequestController = new WebRequestController(
                new GetWebRequestFactory(),
                new WebRequestAssetBundleFactory(),
                new WebRequestTextureFactory(),
                new WebRequestAudioFactory(),
                new PostWebRequestFactory(),
                new PutWebRequestFactory(),
                new PatchWebRequestFactory(),
                new DeleteWebRequestFactory()
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

        public async UniTask<UnityWebRequest> GetAsync(
            string url,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null,
            bool isSigned = false)
        {
            return await SendWebRequest(getWebRequestFactory, url, downloadHandler, onSuccess, onfail, requestAttemps,
                timeout, cancellationToken, headers, isSigned);
        }

        public async UniTask<UnityWebRequest> PatchAsync(
            string url,
            string patchData,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null,
            bool isSigned = false)
        {
            patchWebRequestFactory.SetBody(patchData);
            return await SendWebRequest(patchWebRequestFactory, url, downloadHandler, onSuccess, onFail, requestAttemps,
                timeout, cancellationToken, headers, isSigned);
        }

        public async UniTask<UnityWebRequest> PostAsync(
            string url,
            string postData,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null,
            bool isSigned = false)
        {
            postWebRequestFactory.SetBody(postData);
            return await SendWebRequest(postWebRequestFactory, url, downloadHandler, onSuccess, onfail, requestAttemps,
                timeout, cancellationToken, headers, isSigned);
        }

        public async UniTask<UnityWebRequest> DeleteAsync(
            string url,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null,
            bool isSigned = false)
        {
            return await SendWebRequest(deleteWebRequestFactory, url, downloadHandler, onSuccess, onFail, requestAttemps,
                timeout, cancellationToken, headers, isSigned);
        }

        public async UniTask<UnityWebRequest> GetAssetBundleAsync(
            string url,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default)
        {
            return await SendWebRequest(assetBundleFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, cancellationToken);
        }

        public async UniTask<UnityWebRequest> GetAssetBundleAsync(
            string url,
            Hash128 hash,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default)
        {
            assetBundleFactory.SetHash(hash);
            return await SendWebRequest(assetBundleFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, cancellationToken);
        }

        public async UniTask<UnityWebRequest> GetTextureAsync(
            string url,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool isReadable = true,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> headers = null)
        {
            textureFactory.isReadable = isReadable;
            return await SendWebRequest(textureFactory, url, null, onSuccess, onfail, requestAttemps, timeout, cancellationToken, headers);
        }

        public async UniTask<UnityWebRequest> GetAudioClipAsync(
            string url,
            AudioType audioType,
            Action<UnityWebRequest> onSuccess = null,
            Action<UnityWebRequest> onfail = null,
            int requestAttemps = 3,
            int timeout = 0,
            CancellationToken cancellationToken = default)
        {
            audioClipWebRequestFactory.SetAudioType(audioType);
            return await SendWebRequest(audioClipWebRequestFactory, url, null, onSuccess, onfail, requestAttemps,
                timeout, cancellationToken);
        }


        private async UniTask<UnityWebRequest> SendWebRequest<T>(
            T requestFactory,
            string url,
            DownloadHandler downloadHandler,
            Action<UnityWebRequest> onSuccess,
            Action<UnityWebRequest> onFail,
            int requestAttemps,
            int timeout,
            CancellationToken cancellationToken,
            Dictionary<string, string> headers = null,
            bool isSigned = false) where T : IWebRequestFactory
        {
            requestAttemps = Mathf.Max(1, requestAttemps);

            for (int i = requestAttemps - 1; i >= 0; i--)
            {
                UnityWebRequest request = requestFactory.CreateWebRequest(url);
                request.timeout = timeout;

                if (isSigned)
                {
                    if (!Enum.TryParse(request.method, true, out RequestMethod method))
                        method = RequestMethod.Get;

                    int index = url.IndexOf("?", StringComparison.Ordinal);
                    if (index >= 0)
                        url = url.Substring(0, index);

                    SignBodyResponse signedFetchResponse = await rpcSignRequest.RequestSignedRequest(method, url, null, cancellationToken);

                    await UniTask.SwitchToMainThread();
                    for (var j = 0; j < signedFetchResponse.AuthChain.Count; j++)
                    {
                        request.SetRequestHeader($"x-identity-auth-chain-{j}", signedFetchResponse.AuthChain[j]);
                    }
                    request.SetRequestHeader(TIMESTAMP_HEADER, signedFetchResponse.Timestamp.ToString());
                    request.SetRequestHeader(METADATA_HEADER, signedFetchResponse.Metadata);
                }

                if (headers != null)
                {
                    foreach (var item in headers)
                        request.SetRequestHeader(item.Key, item.Value);
                }

                if (downloadHandler != null)
                    request.downloadHandler = downloadHandler;

                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                await asyncOp.WithCancellation(cancellationToken);

                if (request.WebRequestSucceded())
                {
                    onSuccess?.Invoke(asyncOp.webRequest);
                    return asyncOp.webRequest;
                }
                else if (!request.WebRequestAborted() && request.WebRequestServerError())
                {
                    if (requestAttemps > 0)
                        continue;
                    else
                    {
                        onFail?.Invoke(asyncOp.webRequest);
                        return asyncOp.webRequest;
                    }
                }
                else
                {
                    onFail?.Invoke(asyncOp.webRequest);
                    return asyncOp.webRequest;
                }
            }

            throw new Exception("WebRequestController SendWebRequest: Unexpected error");
        }

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
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
        ) where T : IWebRequestFactory
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = requestFactory.CreateWebRequest(url);
            request.timeout = timeout;

            if (headers != null)
            {
                foreach (var item in headers)
                { request.SetRequestHeader(item.Key, item.Value); }
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
            foreach (var webRequest in ongoingWebRequests)
                webRequest.Dispose();
        }
    }
}
