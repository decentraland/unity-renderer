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

        private readonly UnityWebRequest uwr;
        private byte[] cachedData;
        private bool isDisposed;

        public GltfDownloaderWrapper(UnityWebRequest uwr)
        {
            this.uwr = uwr;
        }

        public bool Success => uwr.result == UnityWebRequest.Result.Success;
        public string Error => uwr.error;

        public byte[] Data
        {
            get
            {
                if (isDisposed)
                    return null;
                if (cachedData != null)
                    return cachedData;

                if (uwr != null)
                    return uwr.downloadHandler.data;
                Debug.LogWarning("The web request was disposed?" + uwr == null);
                return null;
            }
        }

        public string Text => uwr.downloadHandler.text;
        public bool? IsBinary => IsGltfBinary(Data);

        private static bool IsGltfBinary(byte[] data)
        {
            var gltfBinarySignature = BitConverter.ToUInt32(data, 0);
            return gltfBinarySignature == GLB_SIGNATURE;
        }

        public void Dispose()
        {
            isDisposed = true;
            cachedData = null;
            uwr.Dispose();
        }
    }
}
