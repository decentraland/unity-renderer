using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageLabel;

    [SerializeField]
    private Button actionButton;

    [SerializeField]
    private TextMeshProUGUI actionButtonLabel;

    public NotificationModel notificationModel { get; private set; }

    public event System.Action<Notification> OnNotificationDismissed;

    private void OnEnable()
    {
        actionButton.onClick.AddListener(Dismiss);
    }

    private void OnDisable()
    {
        actionButton.onClick.RemoveAllListeners();
    }

    public void Initialize(NotificationModel model)
    {
        notificationModel = model;

        messageLabel.text = notificationModel.message;

        actionButtonLabel.text = notificationModel.buttonMessage;

        if (notificationModel.timer > 0)
        {
            StartCoroutine(TimerCoroutine(notificationModel.timer));
        }

        if (!string.IsNullOrEmpty(notificationModel.scene))
        {
            string sceneID = CommonScriptableObjects.sceneID;
            CurrentSceneUpdated(sceneID, string.Empty);
        }

        if (notificationModel.callback != null)
        {
            actionButton.onClick.AddListener(notificationModel.callback.Invoke);
        }

        if (!string.IsNullOrEmpty(notificationModel.externalCallbackID))
        {
            actionButton.onClick.AddListener(() =>
            {
                // TODO: send message to kernel with callbackID
            });
        }
    }

    private IEnumerator TimerCoroutine(float timer)
    {
        yield return new WaitForSeconds(timer);
        Dismiss();
    }

    private void CurrentSceneUpdated(string current, string previous)
    {
        if (string.CompareOrdinal(current, notificationModel.scene) != 0)
        {
            Dismiss();
        }
    }

    private void Dismiss()
    {
        StopAllCoroutines();
        OnNotificationDismissed?.Invoke(this);
    }
}