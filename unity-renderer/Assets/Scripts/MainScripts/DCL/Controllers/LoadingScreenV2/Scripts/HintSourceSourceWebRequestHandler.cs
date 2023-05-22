using Cysharp.Threading.Tasks;
using UnityEngine;

public class HintSourceSourceWebRequestHandler : ISourceWebRequestHandler
{
    public async UniTask<string> Get(string url)
    {
        Debug.Log($"FD:: HintSourceSourceWebRequestHandler.Get: {url}");
        using (var request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"WebRequest failed: {request.error}");
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }
}
