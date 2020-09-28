using UnityEngine.Networking;

namespace DCL
{
    public interface IWebRequest
    {
        DownloadHandler Get(string url);
        void GetAsync(string url, System.Action<DownloadHandler> OnCompleted, System.Action<string> OnFail);
    }
}