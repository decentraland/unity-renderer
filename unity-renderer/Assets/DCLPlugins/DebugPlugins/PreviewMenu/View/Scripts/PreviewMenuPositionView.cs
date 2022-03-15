using System;
using DCL;
using DCL.NotificationModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;
using Type = DCL.NotificationModel.Type;

public class PreviewMenuPositionView : MonoBehaviour, IDisposable
{
    private const string NOTIFICATION_GROUP = "PositionCopiedToClipboard";
    private const string NOTIFICATION_MESSAGE = "Position copied to clipboard ({0})";

    [SerializeField] internal TMP_InputField xValueInputField;
    [SerializeField] internal TMP_InputField yValueInputField;
    [SerializeField] internal TMP_InputField zValueInputField;
    [SerializeField] internal Button buttonReference;

    private bool isDestroyed;

    private static readonly Model copyPositionToast = new Model()
    {
        type = Type.WARNING,
        groupID = NOTIFICATION_GROUP,
        message = NOTIFICATION_MESSAGE,
        timer = 1.5f
    };

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    private void Awake()
    {
        buttonReference.onClick.AddListener(() =>
        {
            var positionString = $"{xValueInputField.text},{yValueInputField.text},{zValueInputField.text}";
            Environment.i.platform.clipboard
                       .WriteText(positionString);

            var notificationController = NotificationsController.i;
            if (notificationController != null)
            {
                copyPositionToast.message = string.Format(NOTIFICATION_MESSAGE, positionString);
                notificationController.DismissAllNotifications(copyPositionToast.groupID);
                notificationController.ShowNotification(copyPositionToast);
            }
        });
    }

    internal static string FormatFloatValue(float value)
    {
        return $"{value:0.00}";
    }

    internal void LateUpdate()
    {
        Vector3 position = WorldStateUtils.ConvertUnityToScenePosition(CommonScriptableObjects.playerUnityPosition.Get());
        xValueInputField.text = FormatFloatValue(position.x);
        yValueInputField.text = FormatFloatValue(position.y);
        zValueInputField.text = FormatFloatValue(position.z);
    }
}