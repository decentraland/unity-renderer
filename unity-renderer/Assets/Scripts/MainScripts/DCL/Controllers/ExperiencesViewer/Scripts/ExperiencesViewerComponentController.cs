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

        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        isInitialized.Set(view.transform);
    }

    private void IsOpenChanged(bool current, bool previous)
    {
        SetVisibility(current);
    }

    public void SetVisibility(bool visible) { view.SetVisible(visible); }

    public void Dispose()
    {
        isOpen.OnChange -= IsOpenChanged;
    }

    internal virtual ExperiencesViewerComponentView CreateView() => ExperiencesViewerComponentView.Create();
}