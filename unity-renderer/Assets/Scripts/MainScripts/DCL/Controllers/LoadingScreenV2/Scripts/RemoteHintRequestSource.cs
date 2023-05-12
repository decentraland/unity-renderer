using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// We should fetch and parse the hint list from an external URL
    /// If fetch fails, we return an empty list.
    /// </summary>
    public class RemoteHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public List<IHint> loading_hints { get; private set; }
        public SourceTag sourceTag { get; }

        public RemoteHintRequestSource(string source, SourceTag sourceTag)
        {
            this.source = source;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();
        }

        public async UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            // If the CancellationToken is already canceled, return an empty list.
            if(ctx.IsCancellationRequested)
            {
                return loading_hints;
            }

            // TODO:: Fetch the data from the url in the appropriate way
            // FetchDataFromUrl is temporary
            string json = await FetchDataFromUrl(source, ctx);

            if (!string.IsNullOrEmpty(json))
            {
                loading_hints = ParseJsonToHints(json);
            }

            return loading_hints;
        }

        private List<IHint> ParseJsonToHints(string json)
        {
            List<IHint> hints = new List<IHint>();

            // Parse the JSON
            var hintList = JsonUtility.FromJson<List<IHint>>(json);
            foreach (var hint in hintList)
            {
                hints.Add(new BaseHint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag));
            }
            return hints;
        }

        public void Dispose()
        {
            loading_hints.Clear();
        }

        // TODO:: remove temporary mock
        private async UniTask<string> FetchDataFromUrl(string url, CancellationToken ctx)
        {
            string json = string.Empty;

            return json;
        }
    }
}
