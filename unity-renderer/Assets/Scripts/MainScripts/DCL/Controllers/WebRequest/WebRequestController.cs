using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class WebRequestController : IWebRequestController
    {
        private IWebRequest genericWebRequest;
        private IWebRequestAssetBundle assetBundleWebRequest;
        private IWebRequestTexture textureWebRequest;
        private IWebRequestAudio audioClipWebRequest;
        private List<WebRequestAsyncOperation> ongoingWebRequests = new List<WebRequestAsyncOperation>();

        public static WebRequestController Create()
        {
            WebRequestController newWebRequestController = new WebRequestController();

            newWebRequestController.Initialize(
                genericWebRequest: new WebRequest(),
                assetBundleWebRequest: new WebRequestAssetBundle(),
                textureWebRequest: new WebRequestTexture(),
                audioClipWebRequest: new WebRequestAudio());

            return newWebRequestController;
        }

        public void Initialize(
            IWebRequest genericWebRequest,
            IWebRequestAssetBundle assetBundleWebRequest,
            IWebRequestTexture textureWebRequest,
            IWebRequestAudio audioClipWebRequest)
        {
            this.genericWebRequest = genericWebRequest;
            this.assetBundleWebRequest = assetBundleWebRequest;
            this.textureWebRequest = textureWebRequest;
            this.audioClipWebRequest = audioClipWebRequest;
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
            return SendWebRequest(genericWebRequest, url, downloadHandler, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted, headers);
        }

        public WebRequestAsyncOperation GetAssetBundle(
            string url,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            return SendWebRequest(assetBundleWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetAssetBundle(
            string url,
            Hash128 hash,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            assetBundleWebRequest.SetHash(hash);
            return SendWebRequest(assetBundleWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetTexture(
            string url,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            bool isReadable = true)
        {
            textureWebRequest.isReadable = isReadable;
            return SendWebRequest(textureWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetAudioClip(
            string url,
            AudioType audioType,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            audioClipWebRequest.SetAudioType(audioType);
            return SendWebRequest(audioClipWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        private WebRequestAsyncOperation SendWebRequest<T>(
            T requestType,
            string url,
            DownloadHandler downloadHandler,
            Action<IWebRequestAsyncOperation> OnSuccess,
            Action<IWebRequestAsyncOperation> OnFail,
            int requestAttemps,
            int timeout,
            bool disposeOnCompleted,
            Dictionary<string, string> headers = null,
            WebRequestAsyncOperation asyncOp = null
        ) where T : IWebRequest
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = requestType.CreateWebRequest(url);
            request.timeout = timeout;
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.SetRequestHeader(item.Key, item.Value);
                }
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

            UnityWebRequestAsyncOperation requestOp = resultOp.webRequest.SendWebRequest();
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
                            Debug.LogWarning($"Retrying web request: {url} ({remainingAttemps} attemps remaining)");
                            resultOp.Dispose();
                            resultOp = SendWebRequest(requestType, url, downloadHandler, OnSuccess, OnFail, remainingAttemps, timeout, disposeOnCompleted, headers, resultOp);
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
            {
                webRequest.Dispose();
            }
        }
    }
}