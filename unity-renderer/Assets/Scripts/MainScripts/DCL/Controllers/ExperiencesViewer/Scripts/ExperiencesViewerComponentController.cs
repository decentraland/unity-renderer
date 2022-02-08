using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IExperiencesViewerComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}

public class ExperiencesViewerComponentController : IExperiencesViewerComponentController
{
    internal BaseVariable<Transform> isInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;
    public BaseDictionary<string, WearableItem> wearableCatalog => DataStore.i.common.wearables;

    internal ExperiencesViewerComponentView view;
    internal UserProfile userProfile;
    internal CatalogController catalog;
    internal ISceneController sceneController;
    internal Dictionary<string, IParcelScene> activePEXScenes = new Dictionary<string, IParcelScene>();
    internal List<string> pausedPEXScenesIds = new List<string>();

    public void Initialize()
    {
        view = CreateView();
        view.onCloseButtonPressed += OnCloseButtonPressed;
        view.onSomeExperienceUIVisibilityChanged += OnSomeExperienceUIVisibilityChanged;
        view.onSomeExperienceExecutionChanged += OnSomeExperienceExecutionChanged;

        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        sceneController = DCL.Environment.i.world.sceneController;
        sceneController.OnNewPortableExperienceSceneAdded += OnPEXSceneAdded;
        sceneController.OnNewPortableExperienceSceneRemoved += OnPEXSceneRemoved;
        CheckCurrentActivePortableExperiences();

        userProfile = UserProfile.GetOwnUserProfile();
        userProfile.OnUpdate += OnUserProfileUpdated;
        OnUserProfileUpdated(userProfile);

        isInitialized.Set(view.transform);
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
        sceneController.OnNewPortableExperienceSceneAdded -= OnPEXSceneAdded;
        sceneController.OnNewPortableExperienceSceneRemoved -= OnPEXSceneRemoved;
        userProfile.OnUpdate -= OnUserProfileUpdated;
    }

    internal void OnCloseButtonPressed() { SetVisibility(false); }

    internal void OnSomeExperienceUIVisibilityChanged(string pexId, bool isVisible)
    {
        activePEXScenes.TryGetValue(pexId, out IParcelScene scene);
        if (scene != null)
        {
            UIScreenSpace sceneUIComponent = scene.GetSharedComponent<UIScreenSpace>();
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
            if (userProfile.avatar.wearables.Contains(pexId))
            {
                if (!pausedPEXScenesIds.Contains(pexId))
                    pausedPEXScenesIds.Add(pexId);

                WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.ToArray());
            }
            else
            {
                WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.Concat(new List<string> { pexId }).ToArray());
                WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.ToArray());
            }
        }
    }

    internal void IsOpenChanged(bool current, bool previous) { SetVisibility(current); }

    internal void CheckCurrentActivePortableExperiences()
    {
        activePEXScenes.Clear();
        pausedPEXScenesIds.Clear();

        List<GlobalScene> activePortableExperiences = WorldStateUtils.GetActivePortableExperienceScenes();
        foreach (GlobalScene pexScene in activePortableExperiences)
        {
            OnPEXSceneAdded(pexScene);
        }

        numOfLoadedExperiences.Set(activePEXScenes.Count);
    }

    public void OnPEXSceneAdded(IParcelScene scene)
    {
        ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(scene.sceneData.id);

        if (activePEXScenes.ContainsKey(scene.sceneData.id))
        {
            activePEXScenes[scene.sceneData.id] = scene;

            if (experienceToUpdate != null)
                experienceToUpdate.SetUIVisibility(true);

            return;
        }

        GlobalScene newPortableExperienceScene = scene as GlobalScene;

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
                iconUri = newPortableExperienceScene.iconUrl
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

        WebInterface.SetDisabledPortableExperiences(pausedPEXScenesIds.ToArray());
    }

    internal virtual ExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
}