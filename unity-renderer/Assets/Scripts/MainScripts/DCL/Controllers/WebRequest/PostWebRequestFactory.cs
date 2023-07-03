using System.Text;
using UnityEngine.Networking;

namespace DCL
{
    public class PostWebRequestFactory : IPostWebRequestFactory
    {
        private string postData;

        public void SetBody(string data) =>
            this.postData = data;

        public UnityWebRequest CreateWebRequest(string url)
        {
            var unityWebRequest = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            unityWebRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            return unityWebRequest;
        }
    }
}
