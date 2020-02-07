using UnityEngine;

public class GenesisPlazaStep : TutorialStep
{
    const int NOTIFICATION_DURATION = 5;

    public override void OnStepStart()
    {
        base.OnStepStart();

        // NOTE: we should probably remove this toast when proper system for welcome toast is created
        ShowWelcomeToast();
    }

    private void ShowWelcomeToast()
    {
        string notificationText = $"Welcome, {UserProfile.GetOwnUserProfile().userName}!";
        Vector2Int currentCoords = CommonScriptableObjects.playerCoords.Get();
        string parcelName = MinimapMetadata.GetMetadata().GetTile(currentCoords.x, currentCoords.y)?.name;

        if (!string.IsNullOrEmpty(parcelName))
        {
            notificationText += $" You are in {parcelName} {currentCoords.x}, {currentCoords.y}";
        }

        NotificationModel model = new NotificationModel()
        {
            message = notificationText,
            scene = "",
            type = NotificationModel.NotificationType.GENERIC_WITHOUT_BUTTON,
            timer = NOTIFICATION_DURATION
        };

        HUDController.i?.notificationHud.ShowNotification(model);
    }
}
