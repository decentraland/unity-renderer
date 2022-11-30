using System;
using DCL;
using GLTFast;
using GLTFast.Loading;

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

        public GltfDownloaderWrapper(WebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
        }

        public bool success => asyncOp.isSucceded;
        public string error => asyncOp.webRequest.error;
        public byte[] data => cachedData ??= asyncOp.webRequest.downloadHandler.data;
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
            cachedData = null;
            asyncOp.Dispose();
        }
    }
}
