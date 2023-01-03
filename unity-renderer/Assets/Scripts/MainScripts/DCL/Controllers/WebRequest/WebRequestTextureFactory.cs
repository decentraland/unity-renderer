using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestTexture.
    /// </summary>
    public class WebRequestTextureFactory : IWebRequestTextureFactory
    {
        public bool isReadable { get; set; }

        public UnityWebRequest CreateWebRequest(string url)
        {
            return UnityWebRequestTexture.GetTexture(url, !isReadable);
        }
    }
}