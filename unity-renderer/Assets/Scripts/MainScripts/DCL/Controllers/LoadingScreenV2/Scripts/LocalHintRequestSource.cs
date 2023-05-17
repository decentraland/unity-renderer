using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// We should fetch and parse the hint list from a local file
    /// If fetch fails, we return an empty list.
    /// </summary>
    public class LocalHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<IHint> loading_hints { get; private set; }

        private static readonly string LOCAL_HINTS_PATH = "LoadingScreenV2/LocalHintsFallback";

        public LocalHintRequestSource(string sourceJson, SourceTag sourceTag)
        {
            source = sourceJson;
            sourceTag = sourceTag;
            loading_hints = new List<IHint>();

            // TODO:: Parse the JSON
            // var sceneData2 = JsonUtility.FromJson<DataStore_WorldObjects.SceneData>(sceneJson);
            IParcelScene parcelScene = JsonUtility.FromJson<IParcelScene>(sourceJson);

            if (parcelScene == null || parcelScene.sceneData.loadingScreenHints == null) return;

            foreach (var hint in parcelScene.sceneData.loadingScreenHints)
            {
                loading_hints.Add(new BaseHint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag));
            }
        }

        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            return UniTask.FromResult(loading_hints);
        }

        public void Dispose()
        {
            loading_hints.Clear();
        }
    }
}

