using System;
using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    internal class GLTFTextureDownloaderWrapper : ITextureDownload, IDisposable
    {
        private readonly WebRequestAsyncOperation asyncOp;
        private readonly bool nonReadable;
        public GLTFTextureDownloaderWrapper(WebRequestAsyncOperation asyncOp, bool nonReadable)
        {
            this.asyncOp = asyncOp;
            this.nonReadable = nonReadable;
        }
        public bool success => asyncOp.isSucceded;
        public string error => asyncOp.webRequest.error;
        public byte[] data => asyncOp.webRequest.downloadHandler.data;
        public string text => asyncOp.webRequest.downloadHandler.text;
        public bool? isBinary => true;
        public bool MoveNext() => asyncOp.MoveNext();
        public void Reset() => asyncOp.Reset();
        public object Current => asyncOp.Current;
        
        // TODO: Update this when we receive decoded images from the content server
        public Texture2D texture => (asyncOp.webRequest.downloadHandler as DownloadHandlerTexture)?.texture;
        public void Dispose()
        {
            asyncOp.Dispose();
        }
    }
}