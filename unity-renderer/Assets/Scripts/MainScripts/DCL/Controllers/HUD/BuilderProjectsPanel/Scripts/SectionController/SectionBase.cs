using System;
using UnityEngine;

internal abstract class SectionBase : IDisposable
{
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
}