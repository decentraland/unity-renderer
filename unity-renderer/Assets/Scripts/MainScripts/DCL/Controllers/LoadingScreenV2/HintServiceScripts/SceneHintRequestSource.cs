using Cysharp.Threading.Tasks;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     The SceneHintRequestSource class implements the IHintRequestSource interface for retrieving loading hints from a specific scene.
    ///     It monitors the scene loading process, checking the loading status and extracting hints.
    /// </summary>
    public class SceneHintRequestSource : IHintRequestSource
    {
        private const int MAX_WAIT_FOR_SCENE = 1;

        private readonly Vector2Int currentDestination;
        private readonly ISceneController sceneController;
        private LoadParcelScenesMessage.UnityParcelScene currentSceneBeingLoaded;
        private UniTaskCompletionSource<bool> sceneLoadedCompletionSource;

        public SceneHintRequestSource(string sceneJson, SourceTag sourceTag, ISceneController sceneController, Vector2Int currentDestination)
        {
            Source = sceneJson;
            this.SourceTag = sourceTag;
            LoadingHints = new List<Hint>();

            this.sceneController = sceneController;
            this.currentDestination = currentDestination;

            sceneLoadedCompletionSource = new UniTaskCompletionSource<bool>();
            sceneController.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        }

        public string Source { get; }
        public SourceTag SourceTag { get; }
        public List<Hint> LoadingHints { get; }

        public async UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            try
            {
                await UniTask.WhenAny(sceneLoadedCompletionSource.Task, UniTask.Delay(TimeSpan.FromSeconds(MAX_WAIT_FOR_SCENE), cancellationToken: ctx));

                if (ctx.IsCancellationRequested)
                    return LoadingHints;

                bool sceneLoaded = sceneLoadedCompletionSource.Task.Status == UniTaskStatus.Succeeded;

                if (sceneLoaded && currentSceneBeingLoaded?.loadingScreenHints != null && currentSceneBeingLoaded.loadingScreenHints.Count > 0)
                {
                    foreach (Hint basehint in currentSceneBeingLoaded.loadingScreenHints)
                    {
                        if (basehint is Hint hint) { LoadingHints.Add(hint); }
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning($"Exception in SceneHintRequestSource.GetHintsAsync: {ex.Message}\n{ex.StackTrace}"); }

            // Returns an empty list if the scene is not loaded or the hints are not present
            return LoadingHints;
        }

        public void Dispose()
        {
            sceneController.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
            LoadingHints.Clear();
        }

        private void SceneController_OnNewSceneAdded(IParcelScene scene)
        {
            if (scene != null && CheckTargetSceneWithCoords(scene))
            {
                currentSceneBeingLoaded = scene.sceneData;
                sceneLoadedCompletionSource.TrySetResult(true);

                // Recreate the UniTaskCompletionSource to avoid InvalidOperationException on next scene load
                sceneLoadedCompletionSource = new UniTaskCompletionSource<bool>();
            }
        }

        public bool CheckTargetSceneWithCoords(IParcelScene scene) =>
            Environment.i.world.state.GetSceneNumberByCoords(currentDestination).Equals(scene.sceneData.sceneNumber);
    }
}
