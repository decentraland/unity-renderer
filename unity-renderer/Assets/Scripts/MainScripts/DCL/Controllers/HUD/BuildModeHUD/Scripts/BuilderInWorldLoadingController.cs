using System;

public interface IBuilderInWorldLoadingController
{
    bool isActive { get; }

    void Initialize(IBuilderInWorldLoadingView initialLoadingView);
    void Dispose();
    void Show();
    void Hide(bool forzeHidding = false, Action onHideAction = null);
    void SetPercentage(float newValue);
}

public class BuilderInWorldLoadingController : IBuilderInWorldLoadingController
{
    public bool isActive => initialLoadingView.isActive;

    internal IBuilderInWorldLoadingView initialLoadingView;

    public void Initialize(IBuilderInWorldLoadingView initialLoadingView) { this.initialLoadingView = initialLoadingView; }

    public void Dispose() { initialLoadingView.StopTipsCarousel(); }

    public void Show() { initialLoadingView.Show(); }

    public void Hide(bool forzeHidding = false, Action onHideAction = null) { initialLoadingView.Hide(forzeHidding, onHideAction); }

    public void SetPercentage(float newValue) { initialLoadingView.SetPercentage(newValue); }
}