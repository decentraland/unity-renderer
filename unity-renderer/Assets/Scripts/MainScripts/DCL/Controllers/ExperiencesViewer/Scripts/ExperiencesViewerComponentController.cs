using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.ExperiencesViewer
{
    public interface IExperiencesViewerComponentController : IDisposable
    {
        /// <summary>
        /// Initializes the experiences viewer controller.
        /// </summary>
        /// <param name="sceneController">Scene controller used to detect when PEX are created/removed.</param>
        void Initialize(ISceneController sceneController);

        /// <summary>
        /// Set the experience viewer feature as visible or not.
        /// </summary>
        /// <param name="visible">True for showing it.</param>
        void SetVisibility(bool visible);
    }

    public class ExperiencesViewerComponentController : IExperiencesViewerComponentController
    {
        private BaseVariable<Transform> isInitialized => DataStore.i.experiencesViewer.isInitialized;
        private BaseVariable<bool> isOpen => DataStore.i.experiencesViewer.isOpen;
        private BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;
        private BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperiences =>
            DataStore.i.world.disabledPortableExperienceIds;
        private BaseHashSet<string> portableExperienceIds => DataStore.i.world.portableExperienceIds;
        private BaseVariable<string> forcePortableExperience => DataStore.i.world.forcePortableExperience;
        private BaseDictionary<int, bool> isSceneUiEnabled => DataStore.i.HUDs.isSceneUiEnabled;

        internal IExperiencesViewerComponentView view;

        public void Initialize(ISceneController sceneController)
        {
            view = CreateView();
            view.onCloseButtonPressed += OnCloseButtonPressed;
            view.onSomeExperienceUIVisibilityChanged += OnViewRequestToChangeUiVisibility;
            view.onSomeExperienceExecutionChanged += DisableOrEnablePortableExperience;

            isOpen.OnChange += ShowOrHide;
            ShowOrHide(isOpen.Get(), false);

            portableExperienceIds.OnAdded += OnPEXSceneAdded;
            portableExperienceIds.OnRemoved += OnPEXSceneRemoved;
            disabledPortableExperiences.OnAdded += OnPEXDisabled;
            isSceneUiEnabled.OnAdded += OnSomeExperienceUIVisibilityAdded;
            isSceneUiEnabled.OnSet += OnSomeExperienceUIVisibilitySet;

            foreach (var pair in disabledPortableExperiences.Get())
                OnPEXDisabled(pair.Key, pair.Value);

            isInitialized.Set(view.experienceViewerTransform);
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisible(visible);
            isOpen.Set(visible);
        }

        public void Dispose()
        {
            view.onCloseButtonPressed -= OnCloseButtonPressed;
            view.onSomeExperienceUIVisibilityChanged -= OnViewRequestToChangeUiVisibility;
            view.onSomeExperienceExecutionChanged -= DisableOrEnablePortableExperience;
            isOpen.OnChange -= ShowOrHide;

            portableExperienceIds.OnAdded -= OnPEXSceneAdded;
            portableExperienceIds.OnRemoved -= OnPEXSceneRemoved;
            disabledPortableExperiences.OnAdded -= OnPEXDisabled;
            isSceneUiEnabled.OnAdded -= OnSomeExperienceUIVisibilityAdded;
            isSceneUiEnabled.OnSet -= OnSomeExperienceUIVisibilitySet;
        }

        internal void OnCloseButtonPressed() =>
            SetVisibility(false);

        private void OnSomeExperienceUIVisibilityAdded(int pexNumber, bool isVisible)
        {
            IParcelScene scene = GetScene(pexNumber);
            if (scene == null) return;

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
                DataStore.i.HUDs.isSceneUiEnabled.AddOrSet(scene.sceneData.sceneNumber, isVisible);

            if (!isVisible)
                view.ShowUIHiddenToast();
        }

        private void DisableOrEnablePortableExperience(string pexId, bool isPlaying)
        {
            if (isPlaying)
            {
                forcePortableExperience.Set(pexId);

                WebInterface.SetDisabledPortableExperiences(
                    disabledPortableExperiences.GetKeys()
                                               .Where(s => s != pexId)
                                               .ToArray());
            }
            else
            {
                WebInterface.SetDisabledPortableExperiences(disabledPortableExperiences.GetKeys()
                                                                                       .Concat(new[] { pexId })
                                                                                       .Distinct()
                                                                                       .ToArray());
            }
        }

        internal void ShowOrHide(bool current, bool previous) =>
            SetVisibility(current);

        private void OnPEXDisabled(string pxId, (string name, string description, string icon) pex)
        {
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
            Environment.i.world.state.GetPortableExperienceScene(id);

        private IParcelScene GetScene(int sceneNumber) =>
            Environment.i.world.state.GetScene(sceneNumber);

        private void OnPEXSceneAdded(IParcelScene scene)
        {
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(scene.sceneData.id);

            if (experienceToUpdate != null)
                view.RemoveAvailableExperience(scene.sceneData.id);

            GlobalScene newPortableExperienceScene = scene as GlobalScene;
            DataStore.i.experiencesViewer.activeExperience.Get().Add(scene.sceneData.id);

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
            DataStore.i.experiencesViewer.activeExperience.Get().Remove(id);

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
            disabledPortableExperiences.Count()
            + portableExperienceIds.Count();

        internal virtual IExperiencesViewerComponentView CreateView() =>
            ExperiencesViewerComponentView.Create();
    }
}
