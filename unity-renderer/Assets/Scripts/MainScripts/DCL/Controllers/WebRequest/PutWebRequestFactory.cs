using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class PutWebRequestFactory : IPutWebRequestFactory
    {
        private string putData;

        public void SetBody(string data) =>
            this.putData = data;

        public UnityWebRequest CreateWebRequest(string url)
        {
            var unityWebRequest = UnityWebRequest.Put(url, putData);
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            return unityWebRequest;
        }
    }
}
