using System;
using DCL;
using GLTFast.Loading;

namespace DCL
{
    public class GLTFDownloaderWrapper : IDownload, IDisposable
    {
        private readonly WebRequestAsyncOperation asyncOp;
        private byte[] cachedData;
        public GLTFDownloaderWrapper(WebRequestAsyncOperation asyncOp) { this.asyncOp = asyncOp; }
        public bool success => asyncOp.isSucceded;
        public string error => asyncOp.webRequest.error;
        public byte[] data => cachedData ??= asyncOp.webRequest.downloadHandler.data;
        public string text => asyncOp.webRequest.downloadHandler.text;
        public bool? isBinary => IsGltfBinary(data);
        public bool MoveNext() => asyncOp.MoveNext();
        public void Reset() => asyncOp.Reset();
        public object Current => asyncOp.Current;
        
        
        // Note (Kinerius) Totally not stolen from GltfGlobals since it was protected
        internal const uint GLB_MAGIC = 0x46546c67;
        public static bool IsGltfBinary(byte[] data) {
            var magic = BitConverter.ToUInt32( data, 0 );
            return magic == GLB_MAGIC;
        }
        public void Dispose()
        {
            cachedData = null;
            asyncOp.Dispose();
        }
    }
}