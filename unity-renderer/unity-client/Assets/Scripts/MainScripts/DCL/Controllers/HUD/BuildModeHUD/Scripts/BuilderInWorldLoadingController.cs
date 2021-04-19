public interface IBuilderInWorldLoadingController
{
    event System.Action OnCancelLoading;

    void Initialize(IBuilderInWorldLoadingView initialLoadingView);
    void Dispose();
    void Show(bool showTips = true);
    void Hide(bool forzeHidding = false);
    void CancelLoading();
}

public class BuilderInWorldLoadingController : IBuilderInWorldLoadingController
{
    public event System.Action OnCancelLoading;

    internal IBuilderInWorldLoadingView initialLoadingView;

    public void Initialize(IBuilderInWorldLoadingView initialLoadingView)
    {
        this.initialLoadingView = initialLoadingView;
        this.initialLoadingView.OnCancelLoading += CancelLoading;
    }

    public void Dispose()
    {
        initialLoadingView.StopTipsCarousel();
        this.initialLoadingView.OnCancelLoading -= CancelLoading;
    }

    public void Show(bool showTips = true) { initialLoadingView.Show(showTips); }

    public void Hide(bool forzeHidding = false) { initialLoadingView.Hide(forzeHidding); }

    public void CancelLoading() { OnCancelLoading?.Invoke(); }
}