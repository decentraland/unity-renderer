using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using System.Collections;
using DCL.Tutorial;

public class EmailPromptHUDController : IHUD
{
    const float POPUP_DELAY = 60;

    EmailPromptHUDView view;

    bool isPopupRoutineRunning = false;
    Coroutine showPopupDelayedRoutine;

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
        }
        else
        {
            view.showHideAnimator.Hide();
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
    }

    void SetEmailFlag()
    {
        TutorialController.i.SetStepCompleted(TutorialController.TutorialStep.EmailRequested);
    }
}
