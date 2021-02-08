using System;
using UnityEngine;

internal abstract class SectionBase : IDisposable
{
    public bool isVisible { get; private set; } = false;

    public abstract void SetViewContainer(Transform viewContainer);
    public abstract void Dispose();
    public abstract void OnShow();
    public abstract void OnHide();

    public void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        if (visible) OnShow();
        else OnHide();
    }
}