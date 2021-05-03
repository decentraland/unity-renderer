using System;
using UnityEngine;

internal abstract class SectionBase : IDisposable
{
    public bool isVisible { get; private set; } = false;
    public virtual ISectionSearchHandler searchHandler { get; protected set; } = null;
    public virtual SearchBarConfig searchBarConfig { get; protected set; } = new SearchBarConfig()
    {
        showFilterContributor = false,
        showFilterOperator = true,
        showFilterOwner = true,
        showResultLabel = true
    };
    public bool isLoading { get; private set; } = false;

    public abstract void SetViewContainer(Transform viewContainer);
    public abstract void Dispose();
    protected abstract void OnShow();
    protected abstract void OnHide();
    protected virtual void OnFetchingStateChange(bool isLoading) { }

    public void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        if (visible) OnShow();
        else OnHide();
    }

    public void SetFetchingDataState(bool isLoading)
    {
        this.isLoading = isLoading;
        OnFetchingStateChange(isLoading);
    }
}