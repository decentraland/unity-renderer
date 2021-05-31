using UnityEngine;

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
}