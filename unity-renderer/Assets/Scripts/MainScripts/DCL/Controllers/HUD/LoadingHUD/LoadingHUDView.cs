using DCL;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingHUDView : MonoBehaviour, IDisposable
{
    [SerializeField] internal TextMeshProUGUI text;
    [SerializeField] internal Image loadingBar;
    [SerializeField] internal GameObject tipsContainer;
    [SerializeField] internal GameObject noTipsContainer;
    [SerializeField] internal ShowHideAnimator showHideAnimator;
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

    private bool isDestroyed;

    public void Initialize()
    {
        SetMessage("");
        SetPercentage(0);
        SetTips(false);
    }

    private void OnFinishHide(ShowHideAnimator obj)
    {
        showHideAnimator.OnWillFinishHide -= OnFinishHide;
        dataStoreLoadingScreen.Ref.loadingHUD.fadeIn.Set(false);
        dataStoreLoadingScreen.Ref.loadingHUD.fadeOut.Set(false);
        dataStoreLoadingScreen.Ref.loadingHUD.visible.Set(false);
    }

    private void OnFinishStart(ShowHideAnimator obj)
    {
        showHideAnimator.OnWillFinishStart -= OnFinishStart;
        CommonScriptableObjects.isLoadingHUDOpen.Set(true);
        dataStoreLoadingScreen.Ref.loadingHUD.fadeIn.Set(false);
        dataStoreLoadingScreen.Ref.loadingHUD.fadeOut.Set(false);
        dataStoreLoadingScreen.Ref.loadingHUD.visible.Set(true);
    }

    public void SetVisible(bool isVisible, bool instant) {
        if (isVisible)
        {
            showHideAnimator.OnWillFinishStart += OnFinishStart;
            showHideAnimator.Show(instant);
        }
        else
        {
            showHideAnimator.OnWillFinishHide += OnFinishHide;
            showHideAnimator.Hide(instant);
            CommonScriptableObjects.isLoadingHUDOpen.Set(false);
        }
    }

    public void SetMessage(string message) { text.text = message; }
    public void SetPercentage(float percentage) { loadingBar.transform.localScale = new Vector3(percentage, 1, 1); }
    public void SetTips(bool showTips)
    {
        tipsContainer.gameObject.SetActive(showTips);
        noTipsContainer.gameObject.SetActive(!showTips);
    }

    private void OnDestroy() { isDestroyed = true; }

    public void Dispose()
    {
        showHideAnimator.OnWillFinishHide -= OnFinishHide;
        showHideAnimator.OnWillFinishStart -= OnFinishStart;
        if (isDestroyed)
            return;
        isDestroyed = true;
        Destroy(gameObject);
    }
}
