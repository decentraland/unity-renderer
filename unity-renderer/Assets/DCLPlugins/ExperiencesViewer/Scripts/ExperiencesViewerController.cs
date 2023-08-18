using DCL.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.ExperiencesViewer
{
    public class ExperiencesViewerController
    {
        private readonly IExperiencesViewerComponentView view;
        private readonly DataStore dataStore;
        private readonly IWorldState worldState;
        private readonly IPortableExperiencesBridge portableExperiencesBridge;

        private BaseVariable<Transform> isInitialized => dataStore.experiencesViewer.isInitialized;
        private BaseVariable<bool> isOpen => dataStore.experiencesViewer.isOpen;
        private BaseVariable<int> numOfLoadedExperiences => dataStore.experiencesViewer.numOfLoadedExperiences;
        private BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperiences => dataStore.world.disabledPortableExperienceIds;
        private BaseHashSet<string> portableExperienceIds => dataStore.world.portableExperienceIds;
        private BaseVariable<string> forcePortableExperience => dataStore.world.forcePortableExperience;
        private BaseDictionary<int, bool> isSceneUiEnabled => dataStore.HUDs.isSceneUiEnabled;

        public ExperiencesViewerController(IExperiencesViewerComponentView view,
            DataStore dataStore,
            IWorldState worldState,
            IPortableExperiencesBridge portableExperiencesBridge)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.worldState = worldState;
            this.portableExperiencesBridge = portableExperiencesBridge;

            view.OnCloseButtonPressed += OnCloseButtonPressed;
            view.OnExperienceUiVisibilityChanged += OnViewRequestToChangeUiVisibility;
            view.OnExperienceExecutionChanged += DisableOrEnablePortableExperience;

            isOpen.OnChange += ShowOrHide;
            ShowOrHide(isOpen.Get(), false);

            portableExperienceIds.OnAdded += OnPEXSceneAdded;
            portableExperienceIds.OnRemoved += OnPEXSceneRemoved;
            disabledPortableExperiences.OnAdded += OnPEXDisabled;
            isSceneUiEnabled.OnAdded += OnSomeExperienceUIVisibilityAdded;
            isSceneUiEnabled.OnSet += OnSomeExperienceUIVisibilitySet;

            foreach (var pair in disabledPortableExperiences.Get())
                OnPEXDisabled(pair.Key, pair.Value);

            isInitialized.Set(view.ExperienceViewerTransform);
        }

        public void Dispose()
        {
            view.OnCloseButtonPressed -= OnCloseButtonPressed;
            view.OnExperienceUiVisibilityChanged -= OnViewRequestToChangeUiVisibility;
            view.OnExperienceExecutionChanged -= DisableOrEnablePortableExperience;
            isOpen.OnChange -= ShowOrHide;
            portableExperienceIds.OnAdded -= OnPEXSceneAdded;
            portableExperienceIds.OnRemoved -= OnPEXSceneRemoved;
            disabledPortableExperiences.OnAdded -= OnPEXDisabled;
            isSceneUiEnabled.OnAdded -= OnSomeExperienceUIVisibilityAdded;
            isSceneUiEnabled.OnSet -= OnSomeExperienceUIVisibilitySet;
        }

        private void SetVisibility(bool visible)
        {
            view.SetVisible(visible);
            isOpen.Set(visible);
        }

        private void OnCloseButtonPressed() =>
            SetVisibility(false);

        private void OnSomeExperienceUIVisibilityAdded(int pexNumber, bool isVisible)
        {
            IParcelScene scene = GetScene(pexNumber);
            if (scene == null) return;

            // TODO: decouple monobehaviour component from controller
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(scene.sceneData.id);

            if (experienceToUpdate != null)
                experienceToUpdate.SetUIVisibility(isVisible);
        }

        private void OnSomeExperienceUIVisibilitySet(IEnumerable<KeyValuePair<int, bool>> obj)
        {
            foreach ((int sceneNumber, bool visible) in obj)
                OnSomeExperienceUIVisibilityAdded(sceneNumber, visible);
        }

        private void OnViewRequestToChangeUiVisibility(string pexId, bool isVisible)
        {
            IParcelScene scene = GetPortableExperienceScene(pexId);

            if (scene != null)
                dataStore.HUDs.isSceneUiEnabled.AddOrSet(scene.sceneData.sceneNumber, isVisible);

            if (isVisible)
                view.ShowUiShownToast(scene?.GetSceneName());
            else
                view.ShowUiHiddenToast(scene?.GetSceneName());
        }

        private void DisableOrEnablePortableExperience(string pexId, bool isPlaying)
        {
            IParcelScene scene = GetPortableExperienceScene(pexId);

            if (isPlaying)
            {
                forcePortableExperience.Set(pexId);

                portableExperiencesBridge.SetDisabledPortableExperiences(
                    disabledPortableExperiences.GetKeys()
                                               .Where(s => s != pexId)
                                               .ToArray());

                view.ShowEnabledToast(scene?.GetSceneName());
            }
            else
            {
                portableExperiencesBridge.SetDisabledPortableExperiences(disabledPortableExperiences.GetKeys()
                                                                                                    .Concat(new[] { pexId })
                                                                                                    .Distinct()
                                                                                                    .ToArray());

                view.ShowDisabledToast(scene?.GetSceneName());
            }
        }

        private void ShowOrHide(bool current, bool previous) =>
            SetVisibility(current);

        private void OnPEXDisabled(string pxId, (string name, string description, string icon) pex)
        {
            // TODO: decouple monobehaviour component from controller
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(pxId);

            if (experienceToUpdate != null)
            {
                experienceToUpdate.SetName(pex.name);
                experienceToUpdate.SetIcon(pex.icon);
                experienceToUpdate.SetAsPlaying(false);
                return;
            }

            ExperienceRowComponentModel experienceToAdd = new ExperienceRowComponentModel
            {
                id = pxId,
                isPlaying = false,
                isUIVisible = true,
                name = pex.name,
                iconUri = pex.icon,
                allowStartStop = true,
            };

            view.AddAvailableExperience(experienceToAdd);
            numOfLoadedExperiences.Set(GetPortableExperienceCount());
        }

        private void OnPEXSceneAdded(string id) =>
            OnPEXSceneAdded(GetPortableExperienceScene(id));

        private IParcelScene GetPortableExperienceScene(string id) =>
            worldState.GetPortableExperienceScene(id);

        private IParcelScene GetScene(int sceneNumber) =>
            worldState.GetScene(sceneNumber);

        private void OnPEXSceneAdded(IParcelScene scene)
        {
            // TODO: decouple monobehaviour component from controller
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(scene.sceneData.id);

            if (experienceToUpdate != null)
                view.RemoveAvailableExperience(scene.sceneData.id);

            GlobalScene newPortableExperienceScene = scene as GlobalScene;
            dataStore.experiencesViewer.activeExperience.Get().Add(scene.sceneData.id);

            ExperienceRowComponentModel experienceToAdd = new ExperienceRowComponentModel
            {
                id = newPortableExperienceScene.sceneData.id,
                isPlaying = true,
                isUIVisible = true,
                name = newPortableExperienceScene.sceneName,
                iconUri = newPortableExperienceScene.iconUrl,
                allowStartStop = true,
            };

            view.AddAvailableExperience(experienceToAdd);
            numOfLoadedExperiences.Set(GetPortableExperienceCount());

            if (forcePortableExperience.Equals(newPortableExperienceScene.sceneData.id))
                forcePortableExperience.Set(null);
        }

        private void OnPEXSceneRemoved(string id)
        {
            dataStore.experiencesViewer.activeExperience.Get().Remove(id);

            // TODO: decouple monobehaviour component from controller
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(id);

            if (experienceToUpdate != null)
            {
                experienceToUpdate.SetAsPlaying(false);
                return;
            }

            view.RemoveAvailableExperience(id);
            numOfLoadedExperiences.Set(GetPortableExperienceCount());
        }

        private int GetPortableExperienceCount() =>
            disabledPortableExperiences.Count() + portableExperienceIds.Count();
    }
}
