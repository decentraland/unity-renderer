using UnityEngine.Networking;

namespace DCL
{
    public class PostWebRequestFactory : IPostWebRequestFactory
    {
        private string postData;

        public void SetBody(string data) =>
            this.postData = data;

        public UnityWebRequest CreateWebRequest(string url) =>
            UnityWebRequest.Post(url, postData);
    }
}
