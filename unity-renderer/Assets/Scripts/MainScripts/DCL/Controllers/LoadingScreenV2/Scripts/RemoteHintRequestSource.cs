using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    public class RemoteHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<IHint> loading_hints { get; private set; }

        public RemoteHintRequestSource(string sourceUrlJson, SourceTag sourceTag)
        {
            this.source = sourceUrlJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();
        }

        public async UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            try
            {
                // If the CancellationToken is already canceled, return an empty list.
                if (ctx.IsCancellationRequested)
                {
                    return loading_hints;
                }

                string json = await FetchDataFromUrl(source, ctx);

                if (!string.IsNullOrEmpty(json))
                {
                    loading_hints = ParseJsonToHints(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in RemoteHintRequestSource.GetHintsAsync: {ex.Message}\n{ex.StackTrace}");
            }

            return loading_hints;
        }

        private List<IHint> ParseJsonToHints(string json)
        {
            List<IHint> hints = new List<IHint>();

            var hintList = JsonUtility.FromJson<List<BaseHint>>(json);

            if (hintList != null)
            {
                foreach (var hint in hintList)
                {
                    hints.Add(new BaseHint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag));
                }
            }

            return hints;
        }

        public void Dispose()
        {
            loading_hints.Clear();
        }

        private async UniTask<string> FetchDataFromUrl(string url, CancellationToken ctx)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                var ucs = new UniTaskCompletionSource<string>();

                ctx.Register(() =>
                {
                    webRequest.Abort();
                    ucs.TrySetCanceled();
                });

                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, ctx);
                }

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error in RemoteHintRequestSource.FetchDataFromUrl: {webRequest.error}");
                    ucs.TrySetResult(string.Empty);
                }
                else
                {
                    ucs.TrySetResult(webRequest.downloadHandler.text);
                }
                return await ucs.Task;
            }
        }
    }
}
