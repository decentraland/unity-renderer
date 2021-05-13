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
        private IWebRequest textureWebRequest;
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
            IWebRequest textureWebRequest,
            IWebRequestAudio audioClipWebRequest)
        {
            this.genericWebRequest = genericWebRequest;
            this.assetBundleWebRequest = assetBundleWebRequest;
            this.textureWebRequest = textureWebRequest;
            this.audioClipWebRequest = audioClipWebRequest;
        }

        public WebRequestAsyncOperation Get(
            string url,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            return SendWebRequest(genericWebRequest, url, downloadHandler, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetAssetBundle(
            string url,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            return SendWebRequest(assetBundleWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetAssetBundle(
            string url,
            Hash128 hash,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            assetBundleWebRequest.SetHash(hash);
            return SendWebRequest(assetBundleWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetTexture(
            string url,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true)
        {
            return SendWebRequest(textureWebRequest, url, null, OnSuccess, OnFail, requestAttemps, timeout, disposeOnCompleted);
        }

        public WebRequestAsyncOperation GetAudioClip(
            string url,
            AudioType audioType,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
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
            Action<UnityWebRequest> OnSuccess,
            Action<UnityWebRequest> OnFail,
            int requestAttemps,
            int timeout,
            bool disposeOnCompleted) where T : IWebRequest
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = requestType.CreateWebRequest(url);
            request.timeout = timeout;

            if (downloadHandler != null)
                request.downloadHandler = downloadHandler;

            WebRequestAsyncOperation resultOp = new WebRequestAsyncOperation(request);
            resultOp.disposeOnCompleted = disposeOnCompleted;
            ongoingWebRequests.Add(resultOp);

            UnityWebRequestAsyncOperation requestOp = resultOp.webRequest.SendWebRequest();
            requestOp.completed += (asyncOp) =>
            {
                if (!resultOp.isDisposed)
                {
                    if (resultOp.webRequest.WebRequestSucceded())
                    {
                        OnSuccess?.Invoke(resultOp.webRequest);
                        resultOp.SetAsCompleted(true);
                    }
                    else if (!resultOp.webRequest.WebRequestAborted() && resultOp.webRequest.WebRequestServerError())
                    {
                        remainingAttemps--;
                        if (remainingAttemps > 0)
                        {
                            Debug.LogWarning($"Retrying web request: {url} ({remainingAttemps} attemps remaining)");
                            resultOp.Dispose();
                            resultOp = SendWebRequest(requestType, url, downloadHandler, OnSuccess, OnFail, remainingAttemps, timeout, disposeOnCompleted);
                        }
                        else
                        {
                            OnFail?.Invoke(resultOp.webRequest);
                            resultOp.SetAsCompleted(false);
                        }
                    }
                    else
                    {
                        OnFail?.Invoke(resultOp.webRequest);
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