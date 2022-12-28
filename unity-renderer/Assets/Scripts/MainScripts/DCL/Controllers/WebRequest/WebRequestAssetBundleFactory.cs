using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle.
    /// </summary>
    public class WebRequestAssetBundleFactory : IWebRequestAssetBundleFactory
    {
        private bool useHash = false;
        private Hash128 hash;

        public void SetHash(Hash128 hash)
        {
            useHash = true;
            this.hash = hash;
        }

        public UnityWebRequest CreateWebRequest(string url) =>
            useHash
                ? UnityWebRequestAssetBundle.GetAssetBundle(url, hash)
                : UnityWebRequestAssetBundle.GetAssetBundle(url);
    }
}
