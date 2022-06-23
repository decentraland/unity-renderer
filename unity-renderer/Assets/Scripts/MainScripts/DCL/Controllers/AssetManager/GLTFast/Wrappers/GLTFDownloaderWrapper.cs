using GLTFast.Loading;

namespace DCL
{
    internal class GLTFDownloaderWrapper : IDownload
    {
        private readonly WebRequestAsyncOperation asyncOp;
        public GLTFDownloaderWrapper(WebRequestAsyncOperation asyncOp) { this.asyncOp = asyncOp; }
        public bool success => asyncOp.isSucceded;
        public string error => asyncOp.webRequest.error;
        public byte[] data => asyncOp.webRequest.downloadHandler.data;
        public string text => asyncOp.webRequest.downloadHandler.text;
        public bool? isBinary => true;
        public bool MoveNext() => asyncOp.MoveNext();
        public void Reset() => asyncOp.Reset();
        public object Current => asyncOp.Current;
    }
}