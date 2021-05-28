using UnityEngine.Networking;

namespace DCL
{
    public interface IWebRequest
    {
        /// <summary>
        /// Create a UnityWebRequest from a given url.
        /// </summary>
        /// <param name="url">URL from which to create the request.</param>
        /// <returns>A UnityWebRequest ready to be used.</returns>
        UnityWebRequest CreateWebRequest(string url);
    }
}