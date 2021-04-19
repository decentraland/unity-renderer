using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle.
    /// </summary>
    public class WebRequestAssetBundle : IWebRequest
    {
        public UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestAssetBundle.GetAssetBundle(url); }
    }
}