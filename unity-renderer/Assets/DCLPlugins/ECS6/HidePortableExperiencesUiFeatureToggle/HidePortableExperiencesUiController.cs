using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using DCL.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLPlugins.ECS6.HidePortableExperiencesUiFeatureToggle
{
    public class HidePortableExperiencesUiController : IDisposable
    {
        private readonly IWorldState worldState;
        private readonly BaseDictionary<int, bool> isSceneUiEnabled;
        private readonly BaseHashSet<string> portableExperiencesIds;
        private CancellationTokenSource disableUiCancellationToken = new ();

        public HidePortableExperiencesUiController(
            IWorldState worldState,
            BaseDictionary<int, bool> isSceneUiEnabled,
            BaseHashSet<string> portableExperiencesIds)
        {
            this.worldState = worldState;
            this.isSceneUiEnabled = isSceneUiEnabled;
            this.portableExperiencesIds = portableExperiencesIds;

            isSceneUiEnabled.OnAdded += OnSceneUiVisibilityAdded;
            isSceneUiEnabled.OnSet += OnSceneUiVisibilitySet;
            portableExperiencesIds.OnAdded += OnPortableExperienceAdded;
        }

        public void Dispose()
        {
            isSceneUiEnabled.OnAdded -= OnSceneUiVisibilityAdded;
            isSceneUiEnabled.OnSet -= OnSceneUiVisibilitySet;
            portableExperiencesIds.OnAdded -= OnPortableExperienceAdded;

            disableUiCancellationToken.SafeCancelAndDispose();
        }

        private void OnSceneUiVisibilitySet(IEnumerable<KeyValuePair<int, bool>> scenesVisibility)
        {
            foreach ((int sceneNumber, bool visible) in scenesVisibility)
                OnSceneUiVisibilityAdded(sceneNumber, visible);
        }

        private void OnSceneUiVisibilityAdded(int sceneNumber, bool visible)
        {
            IParcelScene currentScene = GetCurrentScene();

            if (currentScene != null)
            {
                if (visible
                    && currentScene.sceneData.scenePortableExperienceFeatureToggles == ScenePortableExperienceFeatureToggles.HideUi)
                {
                    disableUiCancellationToken = disableUiCancellationToken.SafeRestart();
                    DisableUiVisibilityOnNextFrame(sceneNumber, disableUiCancellationToken.Token).Forget();
                    return;
                }
            }

            IParcelScene pxScene = worldState.GetScene(sceneNumber);
            if (pxScene == null) return;
            if (!pxScene.isPortableExperience) return;

            UIScreenSpace sceneUIComponent = pxScene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();

            if (sceneUIComponent != null)
                sceneUIComponent.canvas.enabled = visible;
        }

        private void OnPortableExperienceAdded(string pxId)
        {
            IParcelScene currentScene = GetCurrentScene();

            if (currentScene != null
                && currentScene.sceneData.scenePortableExperienceFeatureToggles != ScenePortableExperienceFeatureToggles.HideUi)
                return;

            IParcelScene pxScene = worldState.GetPortableExperienceScene(pxId);
            if (pxScene == null) return;

            isSceneUiEnabled.AddOrSet(pxScene.sceneData.sceneNumber, false);
        }

        private IParcelScene GetCurrentScene() =>
            worldState.GetScene(worldState.GetCurrentSceneNumber());

        private async UniTaskVoid DisableUiVisibilityOnNextFrame(int sceneNumber, CancellationToken cancellationToken)
        {
            await UniTask.NextFrame(cancellationToken: cancellationToken);
            isSceneUiEnabled.AddOrSet(sceneNumber, false);
        }
    }
}
