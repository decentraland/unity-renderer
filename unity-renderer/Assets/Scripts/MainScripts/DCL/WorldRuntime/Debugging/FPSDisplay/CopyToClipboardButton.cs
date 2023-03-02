using System;
using DCL.NotificationModel;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;
using Type = DCL.NotificationModel.Type;

[RequireComponent(typeof(Button))]
public class CopyToClipboardButton : MonoBehaviour
{

    [SerializeField] private string groupID;
    [SerializeField] private string message;
    [SerializeField] private float timer = 1.5f;
    
    private Func<string> funcToGetContent;
    private Model copyToast;
    private Button button;

    private void Start()
    {
        copyToast = new Model()
        {
            type = Type.WARNING_NO_ICON,
            groupID = groupID,
            message = message,
            timer = timer
        };
        button = GetComponent<Button>();
        button.onClick.AddListener(CopySceneNameToClipboard);
    }
    public void SetFuncToCopy(Func<string> newFunc)
    {
        funcToGetContent = null;
        funcToGetContent += newFunc;
    }
    
    private void CopySceneNameToClipboard()
    {
        string activeSceneName = funcToGetContent?.Invoke();
        Environment.i.platform.clipboard.WriteText(activeSceneName);

        var notificationController = NotificationsController.i;
        if (notificationController != null)
        {
            notificationController.DismissAllNotifications(copyToast.groupID);
            notificationController.ShowNotification(copyToast);
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

}
