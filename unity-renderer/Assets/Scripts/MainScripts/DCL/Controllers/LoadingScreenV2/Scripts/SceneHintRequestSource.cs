using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// This hint request source should fetch the scene.json and load the loading_hints field
    /// If the field is missing, we return an empty list.
    /// </summary>
    public class SceneHintRequestSource : IHintRequestSource
    {
        public List<IHint> loading_hints { get; }
        public string source { get; }
        public SourceTag sourceTag { get; }

        public SceneHintRequestSource(string sceneJson, SourceTag sourceTag)
        {
            source = sceneJson;
            sourceTag = sourceTag;
            loading_hints = new List<IHint>();

            // TODO:: Parse the JSON
            var sceneData2 = JsonUtility.FromJson<DataStore_WorldObjects.SceneData>(sceneJson);
            var sceneData = JsonUtility.FromJson<SceneDataTemp>(sceneJson);

            if (sceneData == null || sceneData.loading_hints == null) return;

            foreach (var hint in sceneData.loading_hints)
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

    // TODO::FD:: temporary class to make the code compile
    public class SceneDataTemp
    {
        public List<IHint> loading_hints;
    }
}

