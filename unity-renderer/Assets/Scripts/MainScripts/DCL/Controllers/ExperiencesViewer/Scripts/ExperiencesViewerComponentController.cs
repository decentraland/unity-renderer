using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IExperiencesViewerComponentController : IDisposable
{
    void Initialize();
    void SetVisibility(bool visible);
    void OnPEXSceneAdded(IParcelScene scene);
    void OnPEXSceneRemoved(string id);
}

public class ExperiencesViewerComponentController : IExperiencesViewerComponentController
{
    internal BaseVariable<Transform> isInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;

    internal ExperiencesViewerComponentView view;
    internal ISceneController sceneController;
    internal Dictionary<string, IParcelScene> activePEXScenes = new Dictionary<string, IParcelScene>();

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

    internal void CheckCurrentActivePortableExperiences()
    {
        activePEXScenes.Clear();

        List<GlobalScene> activePortableExperiences = WorldStateUtils.GetActivePortableExperienceScenes();
        foreach (GlobalScene pexScene in activePortableExperiences)
        {
            OnPEXSceneAdded(pexScene);
        }

        numOfLoadedExperiences.Set(activePEXScenes.Count);
    }

    public void OnPEXSceneAdded(IParcelScene scene)
    {
        if (activePEXScenes.ContainsKey(scene.sceneData.id))
            return;

        GlobalScene newPortableExperienceScene = scene as GlobalScene;

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

    public void OnPEXSceneRemoved(string id)
    {
        if (!activePEXScenes.ContainsKey(id))
            return;

        view.RemoveAvailableExperience(id);
        activePEXScenes.Remove(id);
        numOfLoadedExperiences.Set(activePEXScenes.Count);
    }

    internal virtual ExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
}