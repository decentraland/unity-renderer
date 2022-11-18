using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DCL.NotificationModel;

public class Notification : MonoBehaviour, INotification
{
    [SerializeField] internal TextMeshProUGUI messageLabel;

    [SerializeField] private Button actionButton;

    [SerializeField] private TextMeshProUGUI actionButtonLabel;

    public Model model { get; private set; } = new Model();

    public event Action<INotification> OnNotificationDismissed;

    Coroutine timerCoroutine;

    ShowHideAnimator showHideAnimator;

    private void OnEnable()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(Dismiss);

        AudioScriptableObjects.notification.Play(true);
    }

    private void OnDisable()
    {
        if (actionButton != null)
            actionButton.onClick.RemoveAllListeners();

        StopTimer();

        if (showHideAnimator != null)
            showHideAnimator.OnWillFinishHide -= DismissInternal;
    }

    private void OnDestroy()
    {
        if (showHideAnimator != null)
            showHideAnimator.OnWillFinishHide -= DismissInternal;

        StopTimer();
    }

    public void Show(Model model)
    {
        gameObject.SetActive(true);

        if (showHideAnimator == null)
            showHideAnimator = GetComponent<ShowHideAnimator>();

        if (showHideAnimator != null)
        {
            showHideAnimator.OnWillFinishHide -= DismissInternal;
            showHideAnimator.Show();
        }

        this.model = model;

        if (!string.IsNullOrEmpty(this.model.message))
        {
            messageLabel.text = this.model.message;
        }

        if (!string.IsNullOrEmpty(this.model.buttonMessage))
        {
            actionButtonLabel.text = this.model.buttonMessage;
        }

        if (this.model.timer > 0)
        {
            StopTimer();
            timerCoroutine = CoroutineStarter.Start(TimerCoroutine(this.model.timer));
        }

        if (this.model.scene > 0)
        {
            CurrentSceneUpdated(CommonScriptableObjects.sceneNumber, -1);
        }

        if (actionButton != null)
        {
            if (this.model.callback != null)
            {
                actionButton.onClick.AddListener(this.model.callback.Invoke);
            }

            if (!string.IsNullOrEmpty(this.model.externalCallbackID))
            {
                actionButton.onClick.AddListener(() =>
                {
                    // TODO: send message to kernel with callbackID
                });
            }
        }
    }

    private IEnumerator TimerCoroutine(float timer)
    {
        yield return WaitForSecondsCache.Get(timer);
        Dismiss();
    }

    private void CurrentSceneUpdated(int currentSceneNumber, int previousSceneNumber)
    {
        if (currentSceneNumber != model.scene)
        {
            Dismiss();
        }
    }

    protected virtual void Dismiss() { Dismiss(false); }

    public void Dismiss(bool instant)
    {
        StopTimer();

        if (!instant && showHideAnimator != null)
        {
            showHideAnimator.OnWillFinishHide -= DismissInternal;
            showHideAnimator.OnWillFinishHide += DismissInternal;
            showHideAnimator.Hide();
        }
        else
        {
            DismissInternal();
        }

        if (this != null)
        {
            OnNotificationDismissed?.Invoke(this);
        }
    }

    private void DismissInternal(ShowHideAnimator animator = null)
    {
        if (this == null)
            return;

        if (showHideAnimator != null)
            showHideAnimator.OnWillFinishHide -= DismissInternal;

        if (model.destroyOnFinish)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void StopTimer()
    {
        if (timerCoroutine != null)
        {
            CoroutineStarter.Stop(timerCoroutine);
            timerCoroutine = null;
        }
    }
}