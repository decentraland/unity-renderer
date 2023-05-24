using Cysharp.Threading.Tasks;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// The SceneHintRequestSource class implements the IHintRequestSource interface for retrieving loading hints from a specific scene.
    /// It monitors the scene loading process, checking the loading status and extracting hints.
    /// </summary>
    public class SceneHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<Hint> loading_hints { get; private set; }

        private Vector2Int currentDestination;
        private readonly ISceneController sceneController;
        private LoadParcelScenesMessage.UnityParcelScene currentSceneBeingLoaded;
        private UniTaskCompletionSource<bool> sceneLoadedCompletionSource;

        private const int MAX_WAIT_FOR_SCENE = 1;

        public SceneHintRequestSource(string sceneJson, SourceTag sourceTag, ISceneController sceneController, Vector2Int currentDestination)
        {
            this.source = sceneJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<Hint>();

            this.sceneController = sceneController;
            this.currentDestination = currentDestination;

            sceneLoadedCompletionSource = new UniTaskCompletionSource<bool>();
            sceneController.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        }

        public async UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            try
            {
                await UniTask.WhenAny(sceneLoadedCompletionSource.Task, UniTask.Delay(TimeSpan.FromSeconds(MAX_WAIT_FOR_SCENE), cancellationToken: ctx));

                if (ctx.IsCancellationRequested)
                    return loading_hints;

                bool sceneLoaded = sceneLoadedCompletionSource.Task.Status == UniTaskStatus.Succeeded;

                if (sceneLoaded && currentSceneBeingLoaded?.loadingScreenHints != null && currentSceneBeingLoaded.loadingScreenHints.Count > 0)
                {
                    foreach (var basehint in currentSceneBeingLoaded.loadingScreenHints)
                    {
                        if (basehint is Hint hint)
                        {
                            loading_hints.Add(hint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Exception in SceneHintRequestSource.GetHintsAsync: {ex.Message}\n{ex.StackTrace}");
            }
            // Returns an empty list if the scene is not loaded or the hints are not present
            return loading_hints;
        }

        private void SceneController_OnNewSceneAdded(IParcelScene scene)
        {
            if (scene != null && CheckTargetSceneWithCoords (scene))
            {
                currentSceneBeingLoaded = scene.sceneData;
                sceneLoadedCompletionSource.TrySetResult(true);

                // Recreate the UniTaskCompletionSource to avoid InvalidOperationException on next scene load
                sceneLoadedCompletionSource = new UniTaskCompletionSource<bool>();
            }
        }

        public bool CheckTargetSceneWithCoords (IParcelScene scene)
        {
            return Environment.i.world.state.GetSceneNumberByCoords(currentDestination).Equals(scene.sceneData.sceneNumber);
        }

        public void Dispose()
        {
            sceneController.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
            loading_hints.Clear();
        }
    }
}

