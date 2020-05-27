using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public class Model
    {
        public NotificationFactory.Type type;
        public string message;
        public string buttonMessage;
        public float timer;
        public string scene;
        public System.Action callback;
        public string externalCallbackID;

        public string groupID;
        public bool destroyOnFinish = false;
    }

    [SerializeField]
    internal TextMeshProUGUI messageLabel;

    [SerializeField]
    private Button actionButton;

    [SerializeField]
    private TextMeshProUGUI actionButtonLabel;

    public Notification.Model model { get; private set; } = new Model();

    public event System.Action<Notification> OnNotificationDismissed;

    Coroutine timerCoroutine;

    ShowHideAnimator showHideAnimator;

    private void OnEnable()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(Dismiss);
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


    public void Show(Notification.Model model)
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

        Debug.Log("Notification Initialize... destroy on finish: " + model.destroyOnFinish);

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

        if (!string.IsNullOrEmpty(this.model.scene))
        {
            string sceneID = CommonScriptableObjects.sceneID ?? string.Empty;
            CurrentSceneUpdated(sceneID, string.Empty);
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

    private void CurrentSceneUpdated(string current, string previous)
    {
        if (string.CompareOrdinal(current, model.scene) != 0)
        {
            Dismiss();
        }
    }

    public void Dismiss()
    {
        StopTimer();

        if (showHideAnimator != null)
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
