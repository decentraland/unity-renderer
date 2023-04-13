using UnityEngine.Networking;

namespace DCL
{
    public class DeleteWebRequestFactory : IDeleteWebRequestFactory
    {
        public UnityWebRequest CreateWebRequest(string url) =>
            UnityWebRequest.Delete(url);
    }
}
