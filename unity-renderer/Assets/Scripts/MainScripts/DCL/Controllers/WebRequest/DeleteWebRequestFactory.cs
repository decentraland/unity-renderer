using UnityEngine.Networking;

namespace DCL
{
    public class DeleteWebRequestFactory : IDeleteWebRequestFactory
    {
        public UnityWebRequest CreateWebRequest(string url)
        {
            var unityWebRequest = UnityWebRequest.Delete(url);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            return unityWebRequest;
        }
    }
}
