using UnityEngine.Networking;

namespace DCL
{
    public static class WebRequestExtensions
    {
        public static bool WebRequestSucceded(this UnityWebRequest request) { return request != null && request.result == UnityWebRequest.Result.Success; }
    }
}