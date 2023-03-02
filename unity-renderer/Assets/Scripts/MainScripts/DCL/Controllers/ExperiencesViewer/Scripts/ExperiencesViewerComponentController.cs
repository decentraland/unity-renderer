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
        internal BaseVariable<Transform> isInitialized => DataStore.i.experiencesViewer.isInitialized;
        internal BaseVariable<bool> isOpen => DataStore.i.experiencesViewer.isOpen;
        internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;
        public BaseDictionary<string, WearableItem> wearableCatalog => DataStore.i.common.wearables;

        internal IExperiencesViewerComponentView view;
        internal ISceneController sceneController;
        internal UserProfile userProfile;
        internal Dictionary<string, IParcelScene> activePEXScenes = new Dictionary<string, IParcelScene>();
        internal List<string> pausedPEXScenesIds = new List<string>();
        internal List<string> lastDisablePEXSentToKernel;

        public void Initialize(ISceneController sceneController)
        {
            view = CreateView();
            view.onCloseButtonPressed += OnCloseButtonPressed;
            view.onSomeExperienceUIVisibilityChanged += OnSomeExperienceUIVisibilityChanged;
            view.onSomeExperienceExecutionChanged += OnSomeExperienceExecutionChanged;

            isOpen.OnChange += IsOpenChanged;
            IsOpenChanged(isOpen.Get(), false);

            this.sceneController = sceneController;

            DataStore.i.world.portableExperienceIds.OnAdded += OnPEXSceneAdded;
            DataStore.i.world.portableExperienceIds.OnRemoved += OnPEXSceneRemoved;

            CheckCurrentActivePortableExperiences();

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
            view.onSomeExperienceExecutionChanged -= OnSomeExperienceExecutionChanged;
            isOpen.OnChange -= IsOpenChanged;

            DataStore.i.world.portableExperienceIds.OnAdded -= OnPEXSceneAdded;
            DataStore.i.world.portableExperienceIds.OnRemoved -= OnPEXSceneRemoved;

            if (userProfile != null)
                userProfile.OnUpdate -= OnUserProfileUpdated;
        }

        internal void OnCloseButtonPressed() { SetVisibility(false); }

        internal void OnSomeExperienceUIVisibilityChanged(string pexId, bool isVisible)
        {
            activePEXScenes.TryGetValue(pexId, out IParcelScene scene);
            if (scene != null)
            {
                UIScreenSpace sceneUIComponent = scene.componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>();
                sceneUIComponent.canvas.enabled = isVisible;
            }

            if (!isVisible)
                view.ShowUIHiddenToast();
        }

        internal void OnSomeExperienceExecutionChanged(string pexId, bool isPlaying)
        {
            if (isPlaying)
            {
                WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.Where(x => x != pexId).ToArray());
            }
            else
            {
                // We only keep the experience paused in the list if our avatar has the related wearable equipped
                if (userProfile != null && userProfile.avatar.wearables.Contains(pexId))
                {
                    if (!pausedPEXScenesIds.Contains(pexId))
                        pausedPEXScenesIds.Add(pexId);

                    WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.ToArray());
                }
                else
                {
                    WebInterface.KillPortableExperience(pexId);
                }
            }
        }

        internal void IsOpenChanged(bool current, bool previous) { SetVisibility(current); }

        internal void CheckCurrentActivePortableExperiences()
        {
            activePEXScenes.Clear();
            pausedPEXScenesIds.Clear();

            if (DCL.Environment.i.world.state != null)
            {
                List<GlobalScene> activePortableExperiences =
                    PortableExperienceUtils.GetActivePortableExperienceScenes();
                foreach (GlobalScene pexScene in activePortableExperiences)
                {
                    OnPEXSceneAdded(pexScene);
                }
            }

            numOfLoadedExperiences.Set(activePEXScenes.Count);
        }

        public void OnPEXSceneAdded(string id)
        {
            OnPEXSceneAdded(Environment.i.world.state.GetPortableExperienceScene(id));
        }

        public void OnPEXSceneAdded(IParcelScene scene)
        {
            ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(scene.sceneData.id);

            if (activePEXScenes.ContainsKey(scene.sceneData.id))
            {
                activePEXScenes[scene.sceneData.id] = scene;
                pausedPEXScenesIds.Remove(scene.sceneData.id);

                if (experienceToUpdate != null)
                    experienceToUpdate.SetUIVisibility(true);

                return;
            }

            GlobalScene newPortableExperienceScene = scene as GlobalScene;
            DataStore.i.experiencesViewer.activeExperience.Get().Add(scene.sceneData.id);

            if (pausedPEXScenesIds.Contains(scene.sceneData.id))
            {
                pausedPEXScenesIds.Remove(scene.sceneData.id);

                if (experienceToUpdate != null)
                    experienceToUpdate.SetAsPlaying(true);
            }
            else
            {
                ExperienceRowComponentModel experienceToAdd = new ExperienceRowComponentModel
                {
                    id = newPortableExperienceScene.sceneData.id,
                    isPlaying = true,
                    isUIVisible = true,
                    name = newPortableExperienceScene.sceneName,
                    iconUri = newPortableExperienceScene.iconUrl,
                    allowStartStop = userProfile != null && userProfile.avatar.wearables.Contains(newPortableExperienceScene.sceneData.id)
                };

                view.AddAvailableExperience(experienceToAdd);
                activePEXScenes.Add(scene.sceneData.id, scene);
                numOfLoadedExperiences.Set(activePEXScenes.Count);
            }
        }

        public void OnPEXSceneRemoved(string id)
        {
            if (!activePEXScenes.ContainsKey(id))
                return;

            DataStore.i.experiencesViewer.activeExperience.Get().Remove(id);
            if (pausedPEXScenesIds.Contains(id))
            {
                ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(id);
                if (experienceToUpdate != null)
                {
                    if (!experienceToUpdate.model.isPlaying)
                        return;
                }
            }

            view.RemoveAvailableExperience(id);
            activePEXScenes.Remove(id);
            pausedPEXScenesIds.Remove(id);
            numOfLoadedExperiences.Set(activePEXScenes.Count);
        }

        internal void OnUserProfileUpdated(UserProfile userProfile)
        {
            List<string> experiencesIdsToRemove = new List<string>();

            foreach (var pex in activePEXScenes)
            {
                // We remove from the list all those experiences related to wearables that are not equipped
                if (wearableCatalog.ContainsKey(pex.Key) && !userProfile.avatar.wearables.Contains(pex.Key))
                    experiencesIdsToRemove.Add(pex.Key);
            }

            foreach (string pexId in experiencesIdsToRemove)
            {
                view.RemoveAvailableExperience(pexId);
                activePEXScenes.Remove(pexId);
                pausedPEXScenesIds.Remove(pexId);
            }

            numOfLoadedExperiences.Set(activePEXScenes.Count);

            if (lastDisablePEXSentToKernel != pausedPEXScenesIds)
            {
                lastDisablePEXSentToKernel = pausedPEXScenesIds;
                WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.ToArray());
            }

            List<ExperienceRowComponentView> loadedExperiences = view.GetAllAvailableExperiences();
            for (int i = 0; i < loadedExperiences.Count; i++)
            {
                loadedExperiences[i].SetAllowStartStop(userProfile.avatar.wearables.Contains(loadedExperiences[i].model.id));
            }
        }

        internal virtual IExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
    }
}
