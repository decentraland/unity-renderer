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

        private Vector2Int currentDestination;
        private readonly ISceneController sceneController;
        private IParcelScene currentSceneBeingLoaded;

        public SceneHintRequestSource(string sceneJson, SourceTag sourceTag, ISceneController sceneController, Vector2Int currentDestination)
        {
            this.source = sceneJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();

            this.sceneController = sceneController;
            this.currentDestination = currentDestination;

            sceneController.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        }

        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            return UniTask.FromResult(currentSceneBeingLoaded.sceneData.loadingScreenHints);
        }

        private void SceneController_OnNewSceneAdded(IParcelScene scene)
        {
            if (scene != null && Environment.i.world.state.GetSceneNumberByCoords(currentDestination).Equals(scene.sceneData.sceneNumber))
            {
                currentSceneBeingLoaded = scene;
            }
        }

        public void Dispose()
        {
            sceneController.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
            loading_hints.Clear();
        }
    }
}

