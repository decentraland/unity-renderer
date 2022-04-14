using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// This class manage all our custom WebRequests types.
    /// </summary>
    public interface IWebRequestController : IService
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
            IWebRequestTexture textureWebRequest,
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
        /// <param name="headers">This will set the headers for the request</param>
        IWebRequestAsyncOperation Get(
            string url,
            DownloadHandler downloadHandler = null,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            Dictionary<string, string> headers = null);

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
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true);

        /// <summary>
        /// Download an Asset Bundle from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="hash">Hash to use for caching the AB to disk/indexedDB.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        /// <param name="timeout">Sets the request to attempt to abort after the configured number of seconds have passed (0 = no timeout).</param>
        /// <param name="disposeOnCompleted">Set to true for disposing the request just after it has been completed.</param>
        WebRequestAsyncOperation GetAssetBundle(
            string url,
            Hash128 hash,
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
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
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true,
            bool isReadable = true,
            Dictionary<string, string> headers = null);

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
            Action<IWebRequestAsyncOperation> OnSuccess = null,
            Action<IWebRequestAsyncOperation> OnFail = null,
            int requestAttemps = 3,
            int timeout = 0,
            bool disposeOnCompleted = true);
    }
}