using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequest.
    /// </summary>
    public class GetWebRequestFactory : IWebRequestFactory
    {
        public UnityWebRequest CreateWebRequest(string url) =>
            UnityWebRequest.Get(url);
    }
}
