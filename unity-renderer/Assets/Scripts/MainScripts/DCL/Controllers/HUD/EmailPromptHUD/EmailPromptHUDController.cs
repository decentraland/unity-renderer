using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using System.Collections;

public class EmailPromptHUDController : IHUD
{
    const float POPUP_DELAY = 300;
    const int EMAIL_PROMPT_PROFILE_FLAG = 128;

    EmailPromptHUDView view;

    bool alreadyDisplayed = false;
    bool isPopupRoutineRunning = false;
    Coroutine showPopupDelayedRoutine;

    public EmailPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("EmailPromptHUD")).GetComponent<EmailPromptHUDView>();
        view.name = "_EmailPromptHUD";

        view.OnDismiss += OnDismiss;
        view.OnSendEmail += OnSendEmail;
        CommonScriptableObjects.tutorialActive.OnChange += TutorialActive_OnChange;
        CommonScriptableObjects.motdActive.OnChange += MotdActive_OnChange;

        view.gameObject.SetActive(false);
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            Utils.UnlockCursor();
            alreadyDisplayed = true;
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

        CommonScriptableObjects.emailPromptActive.Set(visible);
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.OnDismiss -= OnDismiss;
            view.OnSendEmail -= OnSendEmail;

            GameObject.Destroy(view.gameObject);
        }
        if (showPopupDelayedRoutine != null)
        {
            StopPopupRoutine();
        }

        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActive_OnChange;
        CommonScriptableObjects.motdActive.OnChange -= MotdActive_OnChange;
    }

    public void SetEnable(bool enable)
    {
        if (enable && !isPopupRoutineRunning && !alreadyDisplayed)
        {
            StartPopupRoutine();
        }
        else if (!enable && isPopupRoutineRunning)
        {
            StopPopupRoutine();
        }
    }

    void ResetPopupDelayed()
    {
        if (isPopupRoutineRunning)
        {
            StopPopupRoutine();
            StartPopupRoutine();
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
        yield return new WaitUntil(() => !CommonScriptableObjects.tutorialActive.Get());
        yield return new WaitUntil(() => !CommonScriptableObjects.motdActive.Get());
        yield return WaitForSecondsCache.Get(seconds);
        yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());
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
        WebInterface.SaveUserTutorialStep(UserProfile.GetOwnUserProfile().tutorialStep | EMAIL_PROMPT_PROFILE_FLAG);
    }

    private void TutorialActive_OnChange(bool current, bool previous)
    {
        if (current)
            ResetPopupDelayed();
    }

    private void MotdActive_OnChange(bool current, bool previous)
    {
        if (current)
            ResetPopupDelayed();
    }
}
