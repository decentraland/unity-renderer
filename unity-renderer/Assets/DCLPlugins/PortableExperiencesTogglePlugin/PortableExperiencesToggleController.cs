using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections.Generic;

namespace DCL.PortableExperiencesToggle
{
    public class PortableExperiencesToggleController : IDisposable
    {
        private readonly IntVariable currentSceneVariable;
        private readonly IWorldState worldState;
        private readonly IPortableExperiencesBridge portableExperiencesBridge;
        private readonly BaseHashSet<string> portableExperiencesIds;
        private readonly BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperiences;
        private readonly BaseDictionary<int, bool> isSceneUiEnabled;
        private readonly List<string> idsBuffer = new ();

        public PortableExperiencesToggleController(IntVariable currentSceneVariable,
            IWorldState worldState,
            IPortableExperiencesBridge portableExperiencesBridge,
            BaseHashSet<string> portableExperiencesIds,
            BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperiences,
            BaseDictionary<int, bool> isSceneUiEnabled)
        {
            this.currentSceneVariable = currentSceneVariable;
            this.worldState = worldState;
            this.portableExperiencesBridge = portableExperiencesBridge;
            this.portableExperiencesIds = portableExperiencesIds;
            this.disabledPortableExperiences = disabledPortableExperiences;
            this.isSceneUiEnabled = isSceneUiEnabled;

            currentSceneVariable.OnChange += OnCurrentSceneChanged;
        }

        public void Dispose()
        {
            currentSceneVariable.OnChange -= OnCurrentSceneChanged;
        }

        private void OnCurrentSceneChanged(int current, int previous)
        {
            IParcelScene scene = worldState.GetScene(current);
            if (scene == null) return;

            if (scene.sceneData.scenePortableExperienceFeatureToggles == ScenePortableExperienceFeatureToggles.Disable)
                DisableAllPortableExperiences();
            else if (scene.sceneData.scenePortableExperienceFeatureToggles == ScenePortableExperienceFeatureToggles.HideUi)
                HideUiOfAllPortableExperiences();
        }

        private void HideUiOfAllPortableExperiences()
        {
            foreach (string pxId in portableExperiencesIds.Get())
            {
                IParcelScene pxScene = worldState.GetPortableExperienceScene(pxId);
                if (pxScene == null) continue;

                isSceneUiEnabled.AddOrSet(pxScene.sceneData.sceneNumber, false);
            }
        }

        private void DisableAllPortableExperiences()
        {
            idsBuffer.Clear();
            idsBuffer.AddRange(disabledPortableExperiences.GetKeys());
            idsBuffer.AddRange(portableExperiencesIds.Get());
            portableExperiencesBridge.SetDisabledPortableExperiences(idsBuffer);
        }
    }
}
