using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestTexture.
    /// </summary>
    public class WebRequestTexture : IWebRequest
    {
        public UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestTexture.GetTexture(url); }
    }
}