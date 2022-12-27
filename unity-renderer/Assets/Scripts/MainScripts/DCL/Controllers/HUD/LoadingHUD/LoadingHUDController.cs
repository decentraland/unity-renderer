using DCL;
using UnityEngine;

public class LoadingHUDController : IHUD
{
    internal LoadingHUDView view;
    internal BaseVariable<bool> visible => DataStore.i.loadingScreen.loadingHUD.visible;
    internal BaseVariable<bool> fadeIn => DataStore.i.loadingScreen.loadingHUD.fadeIn;
    internal BaseVariable<bool> fadeOut => DataStore.i.loadingScreen.loadingHUD.fadeOut;
    internal BaseVariable<string> message => DataStore.i.loadingScreen.loadingHUD.message;
    internal BaseVariable<float> percentage => DataStore.i.loadingScreen.loadingHUD.percentage;
    internal BaseVariable<bool> showTips => DataStore.i.loadingScreen.loadingHUD.showTips;

    protected internal virtual LoadingHUDView CreateView() { return LoadingHUDView.CreateView(); }

    public void Initialize()
    {
        view = CreateView();
        ClearEvents();
        SetViewVisible(fadeIn.Get(), true);
        view?.SetMessage(message.Get());
        view?.SetPercentage(percentage.Get() / 100f);
        view?.SetTips(showTips.Get());

        // set initial states to prevent reconciliation errors
        fadeIn.OnChange += OnFadeInChange;
        fadeOut.OnChange += OnFadeOutChange;

        message.OnChange += OnMessageChanged;
        percentage.OnChange += OnPercentageChanged;
        showTips.OnChange += OnShowTipsChanged;
    }

    private void OnVisibleHUDChanged(bool current, bool previous) { SetViewVisible(current, false); }
    private void OnMessageChanged(string current, string previous) { view?.SetMessage(current); }
    private void OnPercentageChanged(float current, float previous) { view?.SetPercentage(current / 100f); }
    private void OnShowTipsChanged(bool current, bool previous) { view?.SetTips(current); }
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

    public void SetVisibility(bool visible) { this.visible.Set(visible); }

    public void Dispose()
    {
        ClearEvents();
        view?.Dispose();
    }

    internal void ClearEvents()
    {
        visible.OnChange -= OnVisibleHUDChanged;
        message.OnChange -= OnMessageChanged;
        percentage.OnChange -= OnPercentageChanged;
        showTips.OnChange -= OnShowTipsChanged;
    }

    internal void SetViewVisible(bool isVisible, bool instant) { view?.SetVisible(isVisible, instant); }
}
