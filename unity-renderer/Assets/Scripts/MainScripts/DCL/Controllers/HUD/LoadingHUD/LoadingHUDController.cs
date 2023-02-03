using DCL;

public class LoadingHUDController : IHUD
{
    internal LoadingHUDView view;
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

    private BaseVariable<bool> visible => dataStoreLoadingScreen.Ref.loadingHUD.visible;
    private BaseVariable<bool> fadeIn => dataStoreLoadingScreen.Ref.loadingHUD.fadeIn;
    private BaseVariable<bool> fadeOut => dataStoreLoadingScreen.Ref.loadingHUD.fadeOut;
    private BaseVariable<string> message => dataStoreLoadingScreen.Ref.loadingHUD.message;
    private BaseVariable<float> percentage => dataStoreLoadingScreen.Ref.loadingHUD.percentage;
    private BaseVariable<bool> showTips => dataStoreLoadingScreen.Ref.loadingHUD.showTips;

    public LoadingHUDController(LoadingHUDView view)
    {
        this.view = view;
    }

    public void Initialize()
    {
        ClearEvents();
        SetViewVisible(fadeIn.Get(), true);
        view.SetMessage(message.Get());
        view.SetPercentage(percentage.Get() / 100f);
        view.SetTips(showTips.Get());

        // set initial states to prevent reconciliation errors
        fadeIn.OnChange += OnFadeInChange;
        fadeOut.OnChange += OnFadeOutChange;

        message.OnChange += OnMessageChanged;
        percentage.OnChange += OnPercentageChanged;
        showTips.OnChange += OnShowTipsChanged;
    }

    private void OnVisibleHUDChanged(bool current, bool previous)
    {
        SetViewVisible(current, false);
    }

    private void OnMessageChanged(string current, string previous)
    {
        view.SetMessage(current);
    }

    private void OnPercentageChanged(float current, float previous)
    {
        view.SetPercentage(current / 100f);
    }

    private void OnShowTipsChanged(bool current, bool previous)
    {
        view.SetTips(current);
    }

    private void OnFadeInChange(bool current, bool previous)
    {
        if (current)
            SetViewVisible(true, false);
    }

    private void OnFadeOutChange(bool current, bool previous)
    {
        if (current)
            SetViewVisible(false, false);
    }

    public void SetVisibility(bool visible)
    {
        this.visible.Set(visible);
    }

    public void Dispose()
    {
        ClearEvents();
        view.Dispose();
    }

    private void ClearEvents()
    {
        visible.OnChange -= OnVisibleHUDChanged;
        message.OnChange -= OnMessageChanged;
        percentage.OnChange -= OnPercentageChanged;
        showTips.OnChange -= OnShowTipsChanged;
    }

    private void SetViewVisible(bool isVisible, bool instant)
    {
        view.SetVisible(isVisible, instant);
    }
}
