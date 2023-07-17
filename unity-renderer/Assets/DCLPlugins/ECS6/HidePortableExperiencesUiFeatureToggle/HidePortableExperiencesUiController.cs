using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;

namespace DCLPlugins.ECS6.HidePortableExperiencesUiFeatureToggle
{
    public class HidePortableExperiencesUiController : IDisposable
    {
        private readonly IWorldState worldState;
        private readonly BaseDictionary<int, bool> isSceneUiEnabled;

        public HidePortableExperiencesUiController(
            IWorldState worldState,
            BaseDictionary<int, bool> isSceneUiEnabled)
        {
            this.worldState = worldState;
            this.isSceneUiEnabled = isSceneUiEnabled;

            isSceneUiEnabled.OnAdded += OnSceneUiVisibilityAdded;
            isSceneUiEnabled.OnSet += OnSceneUiVisibilitySet;
        }

        public void Dispose()
        {
            isSceneUiEnabled.OnAdded -= OnSceneUiVisibilityAdded;
            isSceneUiEnabled.OnSet -= OnSceneUiVisibilitySet;
        }

        private void OnSceneUiVisibilitySet(IEnumerable<KeyValuePair<int, bool>> scenesVisibility)
        {
            foreach ((int sceneNumber, bool visible) in scenesVisibility)
                OnSceneUiVisibilityAdded(sceneNumber, visible);
        }

        private void OnSceneUiVisibilityAdded(int sceneNumber, bool visible)
        {
            IParcelScene currentScene = worldState.GetScene(worldState.GetCurrentSceneNumber());

            if (currentScene != null)
            {
                if (visible
                    && currentScene.sceneData.scenePortableExperienceFeatureToggles == ScenePortableExperienceFeatureToggles.HideUi)
                    return;
            }

            IParcelScene pxScene = worldState.GetScene(sceneNumber);
            if (pxScene == null) return;
            if (!pxScene.isPortableExperience) return;

            UIScreenSpace sceneUIComponent = pxScene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();

            if (sceneUIComponent != null)
                sceneUIComponent.canvas.enabled = visible;
        }
    }
}
