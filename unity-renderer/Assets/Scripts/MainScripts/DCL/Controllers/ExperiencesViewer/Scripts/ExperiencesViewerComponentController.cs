using DCL;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public interface IExperiencesViewerComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
}

public class ExperiencesViewerComponentController : IExperiencesViewerComponentController
{
    internal BaseVariable<Transform> isInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isOpen => DataStore.i.experiencesViewer.isOpen;

    internal ExperiencesViewerComponentView view;
    internal UserProfile userProfile;
    internal Dictionary<string, ExperienceRowComponentModel> loadedExperiences = new Dictionary<string, ExperienceRowComponentModel>();

    public void Initialize()
    {
        view = CreateView();

        view.onCloseButtonPressed += OnCloseButtonPressed;
        view.onSomeExperienceUIVisibilityChanged += OnSomeExperienceUIVisibilityChanged;
        view.onSomeExperienceExecutionChanged += OnSomeExperienceExecutionChanged;
        
        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        isInitialized.Set(view.transform);

        userProfile = UserProfile.GetOwnUserProfile();
        userProfile.OnUpdate += OnUserProfileUpdated;
        OnUserProfileUpdated(userProfile);

        CatalogController.wearableCatalog.OnAdded += AddExperience;
        CatalogController.wearableCatalog.OnRemoved += RemoveExperience;

        FirstTimeLoading();
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisible(visible);
        isOpen.Set(visible);
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.onCloseButtonPressed -= OnCloseButtonPressed;
            view.onSomeExperienceUIVisibilityChanged -= OnSomeExperienceUIVisibilityChanged;
            view.onSomeExperienceExecutionChanged -= OnSomeExperienceExecutionChanged;
        }

        userProfile.OnUpdate -= OnUserProfileUpdated;
        CatalogController.wearableCatalog.OnAdded -= AddExperience;
        CatalogController.wearableCatalog.OnRemoved -= RemoveExperience;
    }

    internal void OnCloseButtonPressed() { SetVisibility(false); }

    internal void OnSomeExperienceUIVisibilityChanged(string pexId, bool isVisible)
    {
        // TODO...
    }

    internal void OnSomeExperienceExecutionChanged(string pexId, bool isPlaying)
    {
        // TODO...
    }

    internal void IsOpenChanged(bool current, bool previous) { SetVisibility(current); }

    internal void OnUserProfileUpdated(UserProfile userProfile)
    {
        if (string.IsNullOrEmpty(userProfile.userId))
            return;

        userProfile.OnUpdate -= OnUserProfileUpdated;
        CatalogController.RequestOwnedWearables(userProfile.userId);
    }

    internal void FirstTimeLoading()
    {
        List<WearableItem> smartWearables = CatalogController.wearableCatalog
            .Where(x => x.Value.IsSmart())
            .Select(x => x.Value).ToList();

        foreach (WearableItem wearable in smartWearables)
        {
            AddExperience(wearable.id, wearable);
        }
    }

    internal void AddExperience(string id, WearableItem wearable)
    {
        if (!wearable.IsSmart() || loadedExperiences.ContainsKey(id))
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
        loadedExperiences.Add(id, experienceToAdd);
    }

    internal void RemoveExperience(string id, WearableItem wearable)
    {
        if (!wearable.IsSmart() || !loadedExperiences.ContainsKey(id))
            return;

        view.RemoveAvailableExperience(id);
        loadedExperiences.Remove(id);
    }

    internal virtual ExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
}