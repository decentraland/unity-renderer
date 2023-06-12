using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     HintSourceSourceWebRequestHandler asynchronously retrieve data from a given URL.
    /// </summary>
    public class HintSourceSourceWebRequestHandler : ISourceWebRequestHandler
    {
        public async UniTask<string> Get(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"HintSourceSourceWebRequestHandler WebRequest failed: {request.error}");
                    return null;
                }

                return request.downloadHandler.text;
            }
        }
    }
}
