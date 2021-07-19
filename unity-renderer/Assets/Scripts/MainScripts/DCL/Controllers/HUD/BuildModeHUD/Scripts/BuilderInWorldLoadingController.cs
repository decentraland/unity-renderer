using System;
using UnityEngine;

public interface IBuilderInWorldLoadingController
{
    bool isActive { get; }

    void Initialize();
    void Dispose();
    void Show();
    void Hide(bool forzeHidding = false, Action onHideAction = null);
    void SetPercentage(float newValue);
}

public class BuilderInWorldLoadingController : IBuilderInWorldLoadingController
{
    public bool isActive => initialLoadingView.isActive;

    internal IBuilderInWorldLoadingView initialLoadingView;

    private const string VIEW_PATH = "BuilderInWorldLoadingView";

    public void Initialize() { AssignMainView(CreateView()); }

    public void Initialize(IBuilderInWorldLoadingView view) { AssignMainView(view); }

    private void AssignMainView(IBuilderInWorldLoadingView view)
    {
        initialLoadingView = view;

        if (initialLoadingView.viewGO != null)
            initialLoadingView.viewGO.SetActive(false);
    }

    private IBuilderInWorldLoadingView CreateView()
    {
        var view = GameObject.Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<IBuilderInWorldLoadingView>();
        view.viewGO.name = "_BuildModeLoadingHUD";

        return view;
    }

    public void Dispose()
    {
        initialLoadingView.StopTipsCarousel();
        GameObject.Destroy(initialLoadingView.viewGO);
    }

    public void Show() { initialLoadingView.Show(); }

    public void Hide(bool forzeHidding = false, Action onHideAction = null) { initialLoadingView.Hide(forzeHidding, onHideAction); }

    public void SetPercentage(float newValue) { initialLoadingView.SetPercentage(newValue); }
}