using DCL;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.Components;

public interface IExperiencesViewerComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
    void OnWearableAdded(string id, WearableItem wearable);
    void OnWearableRemoved(string id, WearableItem wearable);
}

public class ExperiencesViewerComponentController : IExperiencesViewerComponentController
{
    internal BaseVariable<Transform> isInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isOpen => DataStore.i.experiencesViewer.isOpen;

    internal ExperiencesViewerComponentView view;
    internal UserProfile userProfile;
    internal BaseDictionary<string, WearableItem> wearableCatalog;
    internal ISceneController sceneController;
    internal List<string> loadedExperiencesIds = new List<string>();
    internal Dictionary<string, IParcelScene> activePEXScenes = new Dictionary<string, IParcelScene>();

    public void Initialize()
    {
        view = CreateView();
        view.onCloseButtonPressed += OnCloseButtonPressed;
        view.onSomeExperienceUIVisibilityChanged += OnSomeExperienceUIVisibilityChanged;
        view.onSomeExperienceExecutionChanged += OnSomeExperienceExecutionChanged;
        
        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        userProfile = UserProfile.GetOwnUserProfile();
        userProfile.OnUpdate += OnUserProfileUpdated;
        OnUserProfileUpdated(userProfile);

        wearableCatalog = CatalogController.wearableCatalog;
        wearableCatalog.OnAdded += OnWearableAdded;
        wearableCatalog.OnRemoved += OnWearableRemoved;
        CheckCurrentSmartWearables();

        sceneController = DCL.Environment.i.world.sceneController;
        sceneController.OnNewPortableExperienceSceneAdded += OnPEXSceneAdded;
        sceneController.OnNewPortableExperienceSceneRemoved += OnPEXSceneRemoved;
        CheckCurrentActivePortableExperiences();

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
        userProfile.OnUpdate -= OnUserProfileUpdated;
        wearableCatalog.OnAdded -= OnWearableAdded;
        wearableCatalog.OnRemoved -= OnWearableRemoved;
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
    }

    internal void OnSomeExperienceExecutionChanged(string pexId, bool isPlaying)
    {
        if (!isPlaying)
            WebInterface.KillPortableExperience(pexId);
    }

    internal void IsOpenChanged(bool current, bool previous) { SetVisibility(current); }

    internal void OnUserProfileUpdated(UserProfile userProfile)
    {
        if (string.IsNullOrEmpty(userProfile.userId))
            return;

        userProfile.OnUpdate -= OnUserProfileUpdated;
        CatalogController.RequestOwnedWearables(userProfile.userId);
    }

    internal void CheckCurrentSmartWearables()
    {
        loadedExperiencesIds.Clear();

        List<WearableItem> smartWearables = wearableCatalog
            .Where(x => x.Value.IsSmart())
            .Select(x => x.Value).ToList();

        foreach (WearableItem wearable in smartWearables)
        {
            OnWearableAdded(wearable.id, wearable);
        }
    }

    public void OnWearableAdded(string id, WearableItem wearable)
    {
        if (!wearable.IsSmart() || loadedExperiencesIds.Contains(id))
            return;

        ExperienceRowComponentModel experienceToAdd = new ExperienceRowComponentModel
        {
            id = id,
            isPlaying = false,
            isUIVisible = true,
            name = wearable.GetName(),
            iconUri = wearable.ComposeThumbnailUrl()
        };

        view.AddAvailableExperience(experienceToAdd);
        loadedExperiencesIds.Add(id);
    }

    public void OnWearableRemoved(string id, WearableItem wearable)
    {
        if (!wearable.IsSmart() || !loadedExperiencesIds.Contains(id))
            return;

        view.RemoveAvailableExperience(id);
        loadedExperiencesIds.Remove(id);
    }

    internal void CheckCurrentActivePortableExperiences()
    {
        activePEXScenes.Clear();

        List<GlobalScene> activePortableExperiences = WorldStateUtils.GetActivePortableExperienceScenes();
        foreach (GlobalScene pexScene in activePortableExperiences)
        {
            OnPEXSceneAdded(pexScene);
        }
    }

    internal void OnPEXSceneAdded(IParcelScene scene)
    {
        if (activePEXScenes.ContainsKey(scene.sceneData.id))
            return;

        activePEXScenes.Add(scene.sceneData.id, scene);

        ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(scene.sceneData.id);
        if (experienceToUpdate != null)
        {
            experienceToUpdate.SetAsPlaying(true);
            experienceToUpdate.SetUIVisibility(true);
        }
    }

    internal void OnPEXSceneRemoved(string id)
    {
        if (!activePEXScenes.ContainsKey(id))
            return;

        activePEXScenes.Remove(id);

        ExperienceRowComponentView experienceToUpdate = view.GetAvailableExperienceById(id);
        if (experienceToUpdate != null)
        {
            experienceToUpdate.SetAsPlaying(false);
            experienceToUpdate.SetUIVisibility(true);
        }
    }

    internal virtual ExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
}