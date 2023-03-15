using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class PutWebRequestFactory : IPutWebRequestFactory
    {
        private string putData;
        private bool isPatch = false;

        public void SetBody(string data) =>
            this.putData = data;

        public void SetPatchRequest(bool isPatchRequest)
        {
            this.isPatch = isPatchRequest;
        }

        public UnityWebRequest CreateWebRequest(string url)
        {
            var unityWebRequest = UnityWebRequest.Put(url, putData);
            unityWebRequest.SetRequestHeader ("Content-Type", "application/json");

            if (isPatch)
                unityWebRequest.method = "Patch";

            return unityWebRequest;
        }
    }
}
