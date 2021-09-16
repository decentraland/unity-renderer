using System;
using UnityEngine;

public class ExploreV2Feature : PluginFeature
{
    public IExploreV2MenuComponentView view;

    public event Action OnOpen;
    public event Action OnClose;

    public override void Initialize()
    {
        base.Initialize();

        view = CreateView();
        SetVisibility(false);

        view.OnCloseButtonPressed += View_OnCloseButtonPressed;
    }

    public override void Dispose()
    {
        base.Dispose();

        if (view != null)
            GameObject.Destroy(view.go);
    }

    public void SetVisibility(bool visible)
    {
        if (view == null)
            return;

        if (visible && !view.isActive)
            OnOpen?.Invoke();
        else if (!visible && view.isActive)
            OnClose?.Invoke();

        view.SetActive(visible);
    }

    internal void View_OnCloseButtonPressed() { OnClose?.Invoke(); }

    internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}