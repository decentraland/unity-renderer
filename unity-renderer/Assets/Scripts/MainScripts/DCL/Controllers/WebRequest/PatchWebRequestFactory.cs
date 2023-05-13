using System.Text;
using UnityEngine.Networking;

namespace DCL
{
    public class PatchWebRequestFactory : IPatchWebRequestFactory
    {
        private string patchData;

        public void SetBody(string data) =>
            this.patchData = data;

        public UnityWebRequest CreateWebRequest(string url)
        {
            UnityWebRequest webRequest = new (url, "PATCH", (DownloadHandler)new DownloadHandlerBuffer(), (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(patchData)));
            webRequest.SetRequestHeader ("Content-Type", "application/json");
            return webRequest;
        }
    }
}
