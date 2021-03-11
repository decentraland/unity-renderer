using System;
using UnityEngine;

internal abstract class SectionBase : IDisposable
{
    public static event Action OnRequestContextMenuHide;
    public static event Action<SectionsController.SectionId> OnRequestOpenSection;

    public bool isVisible { get; private set; } = false;
    public virtual ISectionSearchHandler searchHandler { get; protected set; } = null;
    public virtual SearchBarConfig searchBarConfig { get; protected set; } = new SearchBarConfig()
    {
        showFilterContributor = true,
        showFilterOperator = true,
        showFilterOwner = true,
        showResultLabel = true
    };

    public abstract void SetViewContainer(Transform viewContainer);
    public abstract void Dispose();
    protected abstract void OnShow();
    protected abstract void OnHide();

    public void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        if (visible) OnShow();
        else OnHide();
    }

    protected void RequestOpenSection(SectionsController.SectionId id)
    {
        OnRequestOpenSection?.Invoke(id);
    }

    protected void RequestHideContextMenu()
    {
        OnRequestContextMenuHide?.Invoke();
    }
}