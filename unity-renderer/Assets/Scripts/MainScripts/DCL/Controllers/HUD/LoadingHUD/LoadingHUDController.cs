using DCL;
using UnityEngine;

public class LoadingHUDController : IHUD
{
    internal LoadingHUDView view;
    internal BaseVariable<bool> visible => DataStore.i.HUDs.loadingHUD.visible;
    internal BaseVariable<bool> fadeIn => DataStore.i.HUDs.loadingHUD.fadeIn;
    internal BaseVariable<bool> fadeOut => DataStore.i.HUDs.loadingHUD.fadeOut;
    internal BaseVariable<string> message => DataStore.i.HUDs.loadingHUD.message;
    internal BaseVariable<float> percentage => DataStore.i.HUDs.loadingHUD.percentage;
    internal BaseVariable<bool> showTips => DataStore.i.HUDs.loadingHUD.showTips;

    protected internal virtual LoadingHUDView CreateView() { return LoadingHUDView.CreateView(); }

    public void Initialize()
    {
        view = CreateView();
        ClearEvents();
        SetViewVisible(fadeIn.Get());
        view?.SetMessage(message.Get());
        view?.SetPercentage(percentage.Get() / 100f);
        view?.SetTips(showTips.Get());

        // set initial states to prevent reconciliation errors
        //visible.OnChange += OnVisibleHUDChanged;
        fadeIn.OnChange += OnFadeInChange;
        fadeOut.OnChange += OnFadeOutChange;
        
        message.OnChange += OnMessageChanged;
        percentage.OnChange += OnPercentageChanged;
        showTips.OnChange += OnShowTipsChanged;
    }

    private void OnVisibleHUDChanged(bool current, bool previous) { SetViewVisible(current); }
    private void OnMessageChanged(string current, string previous) { view?.SetMessage(current); }
    private void OnPercentageChanged(float current, float previous) { view?.SetPercentage(current / 100f); }
    private void OnShowTipsChanged(bool current, bool previous) { view?.SetTips(current); }
    private void OnFadeInChange(bool current, bool previous)
    {
        Debug.Log($"On Fade in change to {current}");

        if (current)
            SetViewVisible(true);
    }
    private void OnFadeOutChange(bool current, bool previous)
    {
        Debug.Log($"On Fade out change to {current}");

        if (current)
            SetViewVisible(false);
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

    internal void SetViewVisible(bool isVisible) 
    {
        Debug.Log($"set view visible {isVisible}");
        view?.SetVisible(isVisible);
        //fadeIn.Set(false);
        //fadeOut.Set(false);
    }
}