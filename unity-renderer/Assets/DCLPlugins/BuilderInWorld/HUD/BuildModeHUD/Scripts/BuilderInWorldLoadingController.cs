using System;
using DCL;
using DCL.Builder;
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
    public bool isActive => initialLoadingView != null && initialLoadingView.isActive;

    internal IBuilderInWorldLoadingView initialLoadingView;

    private const string VIEW_PATH = "BuilderInWorldLoadingView";

    public void Initialize() { AssignMainView(CreateView()); }

    public void Initialize(IBuilderInWorldLoadingView view) { AssignMainView(view); }

    internal void AssignMainView(IBuilderInWorldLoadingView view)
    {
        initialLoadingView = view;

        if (initialLoadingView.gameObject != null)
            initialLoadingView.gameObject.SetActive(false);
    }

    private IBuilderInWorldLoadingView CreateView()
    {
        var view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<IBuilderInWorldLoadingView>();
        view.gameObject.name = "_BuildModeLoadingHUD";

        return view;
    }

    public void Dispose()
    {
        if ( initialLoadingView == null || initialLoadingView.gameObject == null )
            return;

        initialLoadingView.Dispose();
        UnityEngine.Object.Destroy(initialLoadingView.gameObject);
    }

    public void Show() { initialLoadingView.Show(); }

    public void Hide(bool forzeHidding = false, Action onHideAction = null) { initialLoadingView.Hide(forzeHidding, onHideAction); }

    public void SetPercentage(float newValue) { initialLoadingView.SetPercentage(newValue); }
}