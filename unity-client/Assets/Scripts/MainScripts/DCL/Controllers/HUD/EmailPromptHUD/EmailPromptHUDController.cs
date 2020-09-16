using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using System.Collections;
using System;

public class EmailPromptHUDController : IHUD
{
    const float POPUP_DELAY = 60;

    EmailPromptHUDView view;

    bool isPopupRoutineRunning = false;
    Coroutine showPopupDelayedRoutine;

    public event Action OnSetEmailFlag;

    public bool waitForEndOfTutorial { get; set; } = false;

    public EmailPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("EmailPromptHUD")).GetComponent<EmailPromptHUDView>();
        view.name = "_EmailPromptHUD";

        view.OnDismiss += OnDismiss;
        view.OnSendEmail += OnSendEmail;

        view.gameObject.SetActive(false);
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            Utils.UnlockCursor();
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
            WebInterface.ReportAnalyticsEvent("open email popup");

            AudioScriptableObjects.fadeIn.Play(true);
        }
        else
        {
            view.showHideAnimator.Hide();

            AudioScriptableObjects.fadeOut.Play(true);
        }
    }

    public void Dispose()
    {
        if (view != null)
        {
            GameObject.Destroy(view.gameObject);
        }
        if (showPopupDelayedRoutine != null)
        {
            StopPopupRoutine();
        }
    }

    public void SetEnable(bool enable)
    {
        if (enable && !isPopupRoutineRunning)
        {
            StartPopupRoutine();
        }
        else if (!enable && isPopupRoutineRunning)
        {
            StopPopupRoutine();
        }
    }

    void StartPopupRoutine()
    {
        showPopupDelayedRoutine = CoroutineStarter.Start(ShowPopupDelayed(POPUP_DELAY));
    }

    void StopPopupRoutine()
    {
        if (showPopupDelayedRoutine != null)
        {
            CoroutineStarter.Stop(showPopupDelayedRoutine);
            showPopupDelayedRoutine = null;
        }
        isPopupRoutineRunning = false;
    }

    IEnumerator ShowPopupDelayed(float seconds)
    {
        isPopupRoutineRunning = true;
        yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());
        yield return WaitForSecondsCache.Get(seconds);
        yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());
        yield return new WaitUntil(() => !waitForEndOfTutorial);
        SetVisibility(true);
        isPopupRoutineRunning = false;
    }

    void OnSendEmail(string email)
    {
        WebInterface.SendUserEmail(email);
        SetEmailFlag();
        SetVisibility(false);
    }

    void OnDismiss(bool dontAskAgain)
    {
        if (dontAskAgain)
        {
            SetEmailFlag();
        }
        SetVisibility(false);

        WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]{
             new WebInterface.AnalyticsPayload.Property("notAgain", dontAskAgain? "true" : "false")
         };
        WebInterface.ReportAnalyticsEvent("skip email popup", properties);
    }

    void SetEmailFlag()
    {
        OnSetEmailFlag?.Invoke();
    }
}
