using DCL;
using System;
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

    internal ExperiencesViewerComponentView view;

    public void Initialize()
    {
        view = CreateView();

        view.OnCloseButtonPressed += OnCloseButtonPressed;

        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        isInitialized.Set(view.transform);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisible(visible);
        isOpen.Set(visible);
    }

    public void Dispose()
    {
        isOpen.OnChange -= IsOpenChanged;
    }

    internal void OnCloseButtonPressed() { SetVisibility(false); }

    internal void IsOpenChanged(bool current, bool previous) { SetVisibility(current); }

    internal virtual ExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
}