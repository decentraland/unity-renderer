﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.GLTFast.Wrappers
{
    /// <summary>
    /// With this class we replace all of GLTFast web requests with our own
    /// </summary>
    internal class GltFastDownloadProvider : IDownloadProvider, IDisposable
    {
        private readonly IWebRequestController webRequestController;
        private readonly AssetIdConverter fileToUrl;
        private List<IDisposable> disposables = new ();

        public GltFastDownloadProvider(IWebRequestController webRequestController, AssetIdConverter fileToUrl)
        {
            this.webRequestController = webRequestController;
            this.fileToUrl = fileToUrl;
        }

        public async Task<IDownload> Request(Uri uri)
        {
            string finalUrl = uri.OriginalString;

            string fileName = uri.AbsolutePath.Substring(uri.AbsolutePath.LastIndexOf('/') + 1);

            if (fileToUrl(fileName, out string url)) { finalUrl = url; }

            WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation)webRequestController.Get(
                url: finalUrl,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 30,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GltfDownloaderWrapper wrapper = new GltfDownloaderWrapper(asyncOp);
            disposables.Add(wrapper);

            while (wrapper.MoveNext()) { await Task.Yield(); }

            if (!wrapper.Success) { Debug.LogError($"<color=Red>[GLTFast WebRequest Failed]</color> {asyncOp.asyncOp.webRequest.url} {asyncOp.asyncOp.webRequest.error}"); }

            return wrapper;
        }

        public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable)
        {
            string fileName = uri.AbsolutePath.Substring(uri.AbsolutePath.LastIndexOf('/') + 1);
            fileToUrl(fileName, out string url);

            WebRequestAsyncOperation asyncOp = webRequestController.GetTexture(
                url: url,
                timeout: 30,
                disposeOnCompleted: false,
                requestAttemps: 3);

            GltfTextureDownloaderWrapper wrapper = new GltfTextureDownloaderWrapper(asyncOp);
            disposables.Add(wrapper);

            while (wrapper.MoveNext()) { await Task.Yield(); }

            if (!wrapper.Success) { Debug.LogError("[GLTFast Texture WebRequest Failed] " + asyncOp.asyncOp.webRequest.url); }

            return wrapper;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in disposables) { disposable.Dispose(); }

            disposables = null;
        }
    }
}
