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
        private readonly List<string> idsBuffer = new ();

        public PortableExperiencesToggleController(IntVariable currentSceneVariable,
            IWorldState worldState,
            IPortableExperiencesBridge portableExperiencesBridge,
            BaseHashSet<string> portableExperiencesIds,
            BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperiences)
        {
            this.currentSceneVariable = currentSceneVariable;
            this.worldState = worldState;
            this.portableExperiencesBridge = portableExperiencesBridge;
            this.portableExperiencesIds = portableExperiencesIds;
            this.disabledPortableExperiences = disabledPortableExperiences;

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

            if (scene.sceneData.scenePortableExperienceFeatureToggles != ScenePortableExperienceFeatureToggles.Disable) return;

            DisableAllPortableExperiences();
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
