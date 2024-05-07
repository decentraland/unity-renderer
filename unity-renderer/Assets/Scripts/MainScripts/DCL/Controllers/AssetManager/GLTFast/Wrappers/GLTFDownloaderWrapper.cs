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

        private readonly UnityWebRequest webRequest;
        private byte[] cachedData;
        private bool isDisposed;

        public GltfDownloaderWrapper(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
        }

        public bool Success => webRequest.result == UnityWebRequest.Result.Success;
        public string Error => webRequest.error;

        public byte[] Data
        {
            get
            {
                if (isDisposed) return null;
                if (cachedData != null) return cachedData;

                if (webRequest != null) return webRequest.downloadHandler.data;
                return null;
            }
        }

        public string Text => webRequest.downloadHandler.text;
        public bool? IsBinary => IsGltfBinary(Data);

        private static bool IsGltfBinary(byte[] data)
        {
            if (data == null) return false;
            var gltfBinarySignature = BitConverter.ToUInt32(data, 0);
            return gltfBinarySignature == GLB_SIGNATURE;
        }

        public void Dispose()
        {
            isDisposed = true;
            cachedData = null;
            webRequest.Dispose();
        }
    }
}
