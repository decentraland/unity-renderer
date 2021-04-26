using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// This class manage all our custom WebRequests types.
    /// </summary>
    public interface IWebRequestController
    {
        /// <summary>
        /// Initialize the controller with all the request types injected.
        /// </summary>
        /// <param name="genericWebRequest"></param>
        /// <param name="assetBundleWebRequest"></param>
        /// <param name="textureWebRequest"></param>
        /// <param name="audioWebRequest"></param>
        void Initialize(
            IWebRequest genericWebRequest,
            IWebRequestAssetBundle assetBundleWebRequest,
            IWebRequest textureWebRequest,
            IWebRequestAudio audioWebRequest);

        /// <summary>
        /// Download data from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="downloadHandler">Downloader handler to be used by the GET request.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        /// <param name="timeout">Sets the request to attempt to abort after the configured number of seconds have passed (0 = no timeout).</param>
        /// <param name="disposeOnCompleted">Set to true for disposing the request just after it has been completed.</param>
        WebRequestAsyncOperation Get(
            string url,
            DownloadHandler downloadHandler = null,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true);

        /// <summary>
        /// Download an Asset Bundle from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        /// <param name="timeout">Sets the request to attempt to abort after the configured number of seconds have passed (0 = no timeout).</param>
        /// <param name="disposeOnCompleted">Set to true for disposing the request just after it has been completed.</param>
        WebRequestAsyncOperation GetAssetBundle(
            string url,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true);

        /// <summary>
        /// Download a texture from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        /// <param name="timeout">Sets the request to attempt to abort after the configured number of seconds have passed (0 = no timeout).</param>
        /// <param name="disposeOnCompleted">Set to true for disposing the request just after it has been completed.</param>
        WebRequestAsyncOperation GetTexture(
            string url,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true);

        /// <summary>
        /// Download an audio clip from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="audioType">Type of audio that will be requested.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        /// <param name="timeout">Sets the request to attempt to abort after the configured number of seconds have passed (0 = no timeout).</param>
        /// <param name="disposeOnCompleted">Set to true for disposing the request just after it has been completed.</param>
        WebRequestAsyncOperation GetAudioClip(
            string url,
            AudioType audioType,
            Action<UnityWebRequest> OnSuccess = null,
            Action<UnityWebRequest> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true);

        /// <summary>
        /// Abort and clean all the ongoing web requests.
        /// </summary>
        void Dispose();
    }

    public class WebRequestController : IWebRequestController
    {
        public static WebRequestController i { get; private set; }

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
            i = this;

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