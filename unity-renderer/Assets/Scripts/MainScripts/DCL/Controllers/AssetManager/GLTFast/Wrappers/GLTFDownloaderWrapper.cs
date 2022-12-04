using System;
using DCL;
using GLTFast;
using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.GLTFast.Wrappers
{
    public class GltfDownloaderWrapper : IDownload
    {
        /// <summary>
        /// First four bytes of a glTF-Binary file are made up of this signature
        /// Represents glTF in ASCII
        /// </summary>
        private const uint GLB_SIGNATURE = 0x46546c67;

        private readonly WebRequestAsyncOperation asyncOp;
        private byte[] cachedData;
        private bool isDisposed;

        public GltfDownloaderWrapper(WebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
        }

        public bool success => asyncOp.isSucceded;
        public string error => asyncOp.webRequest.error;

        public byte[] data
        {
            get
            {
                if (isDisposed) return null;
                if (cachedData != null) return cachedData;

                UnityWebRequest asyncOpWebRequest = asyncOp.webRequest;
                if (asyncOpWebRequest != null) return asyncOpWebRequest.downloadHandler.data;
                Debug.LogWarning("The web request was disposed?" + asyncOp.isDisposed);
                return null;
            }
        }

        public string text => asyncOp.webRequest.downloadHandler.text;
        public bool? isBinary => IsGltfBinary(data);

        private static bool IsGltfBinary(byte[] data)
        {
            var gltfBinarySignature = BitConverter.ToUInt32(data, 0);
            return gltfBinarySignature == GLB_SIGNATURE;
        }

        public bool MoveNext() =>
            asyncOp.MoveNext();

        public void Dispose()
        {
            isDisposed = true;
            cachedData = null;
            asyncOp.Dispose();
        }
    }
}
