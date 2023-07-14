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
        private readonly IntVariable currentSceneVariable;
        private readonly IWorldState worldState;
        private readonly BaseHashSet<string> portableExperiencesIds;
        private readonly BaseDictionary<int, bool> isSceneUiEnabled;

        public HidePortableExperiencesUiController(IntVariable currentSceneVariable,
            IWorldState worldState,
            BaseHashSet<string> portableExperiencesIds,
            BaseDictionary<int, bool> isSceneUiEnabled)
        {
            this.currentSceneVariable = currentSceneVariable;
            this.worldState = worldState;
            this.portableExperiencesIds = portableExperiencesIds;
            this.isSceneUiEnabled = isSceneUiEnabled;

            currentSceneVariable.OnChange += OnCurrentSceneChanged;
            isSceneUiEnabled.OnAdded += OnSceneUiVisibilityAdded;
            isSceneUiEnabled.OnSet += OnSceneUiVisibilitySet;
        }

        public void Dispose()
        {
            currentSceneVariable.OnChange -= OnCurrentSceneChanged;
            isSceneUiEnabled.OnAdded -= OnSceneUiVisibilityAdded;
            isSceneUiEnabled.OnSet -= OnSceneUiVisibilitySet;
        }

        private void OnCurrentSceneChanged(int current, int previous)
        {
            IParcelScene currentScene = worldState.GetScene(current);
            if (currentScene == null) return;

            if (currentScene.sceneData.scenePortableExperienceFeatureToggles != ScenePortableExperienceFeatureToggles.HideUi) return;

            foreach (string pxId in portableExperiencesIds.Get())
            {
                IParcelScene pxScene = worldState.GetPortableExperienceScene(pxId);
                UIScreenSpace sceneUIComponent = pxScene?.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();

                if (sceneUIComponent != null)
                    sceneUIComponent.canvas.enabled = false;
            }
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
