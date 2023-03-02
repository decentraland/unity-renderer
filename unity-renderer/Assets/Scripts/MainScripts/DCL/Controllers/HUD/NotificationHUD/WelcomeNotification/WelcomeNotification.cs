using DCL.NotificationModel;
using UnityEngine;

public static class WelcomeNotification
{
    const int NOTIFICATION_DURATION = 5;

    public static Model Get()
    {
        string notificationText = $"Welcome, {UserProfile.GetOwnUserProfile().userName}!";
        Vector2Int currentCoords = CommonScriptableObjects.playerCoords.Get();
        string parcelName = MinimapMetadata.GetMetadata().GetSceneInfo(currentCoords.x, currentCoords.y)?.name;

        if (!string.IsNullOrEmpty(parcelName))
        {
            notificationText += $" You are in {parcelName} {currentCoords.x}, {currentCoords.y}";
        }

        return new Model()
        {
            message = notificationText,
            scene = -1,
            type = Type.GENERIC_WITHOUT_BUTTON,
            timer = NOTIFICATION_DURATION
        };
    }
}