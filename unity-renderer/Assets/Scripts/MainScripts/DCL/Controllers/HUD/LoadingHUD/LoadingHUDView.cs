using System;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingHUDView : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI text;
    [SerializeField] internal Image loadingBar;
    [SerializeField] internal GameObject tipsContainer;
    [SerializeField] internal GameObject noTipsContainer;
    [SerializeField] internal ShowHideAnimator showHideAnimator;

    private bool isDestroyed = false;

    public static LoadingHUDView CreateView()
    {
        LoadingHUDView view = Instantiate(Resources.Load<GameObject>("LoadingHUD")).GetComponent<LoadingHUDView>();
        view.gameObject.name = "_LoadingHUD";
        return view;
    }

    public void Initialize()
    {
        SetMessage("");
        SetPercentage(0);
        SetTips(false);
    }

    private void OnFinishHide(ShowHideAnimator obj)
    {
        showHideAnimator.OnWillFinishHide -= OnFinishHide;
        DataStore.i.HUDs.loadingHUD.fadeIn.Set(false);
        DataStore.i.HUDs.loadingHUD.fadeOut.Set(false);
        DataStore.i.HUDs.loadingHUD.visible.Set(false);
    }

    private void OnFinishStart(ShowHideAnimator obj)
    {
        showHideAnimator.OnWillFinishStart -= OnFinishStart;
        CommonScriptableObjects.isLoadingHUDOpen.Set(true);
        DataStore.i.HUDs.loadingHUD.fadeIn.Set(false);
        DataStore.i.HUDs.loadingHUD.fadeOut.Set(false);
        DataStore.i.HUDs.loadingHUD.visible.Set(true);
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