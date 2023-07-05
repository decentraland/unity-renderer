using DCL.Components;
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

        internal IExperiencesViewerComponentView view;
        private UserProfile userProfile;
        private List<string> lastDisablePEXSentToKernel;

        public void Initialize(ISceneController sceneController)
        {
            view = CreateView();
            view.onCloseButtonPressed += OnCloseButtonPressed;
            view.onSomeExperienceUIVisibilityChanged += OnSomeExperienceUIVisibilityChanged;
            view.onSomeExperienceExecutionChanged += DisableOrEnablePortableExperience;

            isOpen.OnChange += ShowOrHide;
            ShowOrHide(isOpen.Get(), false);

            DataStore.i.world.portableExperienceIds.OnAdded += OnPEXSceneAdded;
            DataStore.i.world.portableExperienceIds.OnRemoved += OnPEXSceneRemoved;
            DataStore.i.world.disabledPortableExperienceIds.OnAdded += OnPEXDisabled;

            // CheckCurrentActivePortableExperiences();

            userProfile = UserProfile.GetOwnUserProfile();

            if (userProfile != null)
            {
                userProfile.OnUpdate += OnUserProfileUpdated;
                OnUserProfileUpdated(userProfile);
            }

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
            view.onSomeExperienceUIVisibilityChanged -= OnSomeExperienceUIVisibilityChanged;
            view.onSomeExperienceExecutionChanged -= DisableOrEnablePortableExperience;
            isOpen.OnChange -= ShowOrHide;

            DataStore.i.world.portableExperienceIds.OnAdded -= OnPEXSceneAdded;
            DataStore.i.world.portableExperienceIds.OnRemoved -= OnPEXSceneRemoved;
            DataStore.i.world.disabledPortableExperienceIds.OnAdded -= OnPEXDisabled;

            if (userProfile != null)
                userProfile.OnUpdate -= OnUserProfileUpdated;
        }

        internal void OnCloseButtonPressed()
        {
            SetVisibility(false);
        }

        private void OnSomeExperienceUIVisibilityChanged(string pexId, bool isVisible)
        {
            IParcelScene scene = GetPortableExperienceScene(pexId);

            if (scene != null)
            {
                UIScreenSpace sceneUIComponent = scene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();

                if (sceneUIComponent != null)
                    sceneUIComponent.canvas.enabled = isVisible;
                else
                    Debug.LogError($"Cannot find UIScreenSpace component to change the PX visibility: {pexId}");
            }

            if (!isVisible)
                view.ShowUIHiddenToast();
        }

        private void DisableOrEnablePortableExperience(string pexId, bool isPlaying)
        {
            if (isPlaying)
            {
                WebInterface.SetDisabledPortableExperiences(
                    DataStore.i.world.disabledPortableExperienceIds.Get()
                             .Where(s => s != pexId)
                             .ToArray());
            }
            else
            {
                if (!DataStore.i.world.disabledPortableExperienceIds.Contains(pexId))
                {
                    WebInterface.SetDisabledPortableExperiences(DataStore.i.world.disabledPortableExperienceIds.Get()
                                                                         .Concat(new[] { pexId })
                                                                         .ToArray());
                }
            }
        }

        internal void ShowOrHide(bool current, bool previous)
        {
            SetVisibility(current);
        }

        // internal void CheckCurrentActivePortableExperiences()
        // {
        //     activePEXScenes.Clear();
        //     pausedPEXScenesIds.Clear();
        //
        //     if (Environment.i.world.state != null)
        //     {
        //         List<GlobalScene> activePortableExperiences =
        //             PortableExperienceUtils.GetActivePortableExperienceScenes();
        //
        //         foreach (GlobalScene pexScene in activePortableExperiences)
        //             OnPEXSceneAdded(pexScene);
        //     }
        //
        //     numOfLoadedExperiences.Set(activePEXScenes.Count);
        // }

        private void OnPEXDisabled(string pxId)
        {
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(pxId);

            if (experienceToUpdate != null)
            {
                experienceToUpdate.SetAsPlaying(false);
                return;
            }

            ExperienceRowComponentModel experienceToAdd = new ExperienceRowComponentModel
            {
                id = pxId,
                isPlaying = false,
                isUIVisible = true,
                name = "TODO",
                iconUri = "",
                allowStartStop = true,
            };

            view.AddAvailableExperience(experienceToAdd);
            numOfLoadedExperiences.Set(GetPortableExperienceCount());
        }

        private void OnPEXSceneAdded(string id)
        {
            OnPEXSceneAdded(GetPortableExperienceScene(id));
        }

        private IParcelScene GetPortableExperienceScene(string id) =>
            Environment.i.world.state.GetPortableExperienceScene(id);

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

        private void OnUserProfileUpdated(UserProfile userProfile)
        {
            // List<string> experiencesIdsToRemove = new List<string>();
            //
            // foreach (var pex in activePEXScenes)
            // {
            //     // We remove from the list all those experiences related to wearables that are not equipped
            //     if (wearableCatalog.ContainsKey(pex.Key) && !userProfile.avatar.wearables.Contains(pex.Key))
            //         experiencesIdsToRemove.Add(pex.Key);
            // }
            //
            // foreach (string pexId in experiencesIdsToRemove)
            // {
            //     view.RemoveAvailableExperience(pexId);
            //     activePEXScenes.Remove(pexId);
            //     pausedPEXScenesIds.Remove(pexId);
            // }
            //
            // numOfLoadedExperiences.Set(activePEXScenes.Count);
            //
            // if (lastDisablePEXSentToKernel != pausedPEXScenesIds)
            // {
            //     lastDisablePEXSentToKernel = pausedPEXScenesIds;
            //     WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.ToArray());
            // }
            //
            // List<ExperienceRowComponentView> loadedExperiences = view.GetAllAvailableExperiences();
            //
            // for (int i = 0; i < loadedExperiences.Count; i++) { loadedExperiences[i].SetAllowStartStop(userProfile.avatar.wearables.Contains(loadedExperiences[i].model.id)); }
        }

        private int GetPortableExperienceCount() =>
            DataStore.i.world.disabledPortableExperienceIds.Count()
            + DataStore.i.world.portableExperienceIds.Count();

        internal virtual IExperiencesViewerComponentView CreateView() =>
            ExperiencesViewerComponentView.Create();
    }
}
