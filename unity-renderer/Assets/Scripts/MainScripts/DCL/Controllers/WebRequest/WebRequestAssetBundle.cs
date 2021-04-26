using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle.
    /// </summary>
    public interface IWebRequestAssetBundle : IWebRequest
    {
        /// <summary>
        /// Configure a version hash for the request.
        /// </summary>
        /// <param name="hash">A version hash.</param>
        void SetHash(Hash128 hash);
    }

    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle.
    /// </summary>
    public class WebRequestAssetBundle : IWebRequestAssetBundle
    {
        private bool useHash = false;
        private Hash128 hash;

        public void SetHash(Hash128 hash)
        {
            useHash = true;
            this.hash = hash;
        }

        public UnityWebRequest CreateWebRequest(string url)
        {
            if (useHash)
                return UnityWebRequestAssetBundle.GetAssetBundle(url, hash);
            else
                return UnityWebRequestAssetBundle.GetAssetBundle(url);
        }
    }
}