using UnityEngine;

public static class NotificationScriptableObjects
{
    private static FloatVariable newApprovedFriendsValue;
    public static FloatVariable newApprovedFriends => CommonScriptableObjects.GetOrLoad(ref newApprovedFriendsValue, "ScriptableObjects/NotificationBadge_NewApprovedFriends");

    private static FloatVariable pendingChatMessagesValue;
    public static FloatVariable pendingChatMessages => CommonScriptableObjects.GetOrLoad(ref pendingChatMessagesValue, "ScriptableObjects/NotificationBadge_PendingChatMessages");

    private static FloatVariable pendingFriendRequestsValue;
    public static FloatVariable pendingFriendRequests => CommonScriptableObjects.GetOrLoad(ref pendingFriendRequestsValue, "ScriptableObjects/NotificationBadge_PendingFriendRequests");
}

public static class CommonScriptableObjects
{
    private static Vector3Variable playerUnityPositionValue;
    public static Vector3Variable playerUnityPosition => GetOrLoad(ref playerUnityPositionValue, "ScriptableObjects/PlayerUnityPosition");

    private static Vector3Variable playerWorldPositionValue;
    public static Vector3Variable playerWorldPosition => GetOrLoad(ref playerWorldPositionValue, "ScriptableObjects/PlayerWorldPosition");

    private static Vector3Variable playerUnityEulerAnglesValue;
    public static Vector3Variable playerUnityEulerAngles => GetOrLoad(ref playerUnityEulerAnglesValue, "ScriptableObjects/PlayerUnityEulerAngles");

    private static Vector3Variable playerUnityToWorldOffsetValue;
    public static Vector3Variable playerUnityToWorldOffset => GetOrLoad(ref playerUnityToWorldOffsetValue, "ScriptableObjects/PlayerUnityToWorldOffset");

    private static Vector2IntVariable playerCoordsValue;
    public static Vector2IntVariable playerCoords => GetOrLoad(ref playerCoordsValue, "ScriptableObjects/PlayerCoords");

    private static StringVariable sceneIDValue;
    public static StringVariable sceneID => GetOrLoad(ref sceneIDValue, "ScriptableObjects/SceneID");

    private static FloatVariable minimapZoomValue;
    public static FloatVariable minimapZoom => GetOrLoad(ref minimapZoomValue, "ScriptableObjects/MinimapZoom");

    private static Vector3NullableVariable characterForwardValue;
    public static Vector3NullableVariable characterForward => GetOrLoad(ref characterForwardValue, "ScriptableObjects/CharacterForward");

    private static Vector3Variable cameraForwardValue;
    public static Vector3Variable cameraForward => GetOrLoad(ref cameraForwardValue, "ScriptableObjects/CameraForward");

    private static Vector3Variable cameraPositionValue;
    public static Vector3Variable cameraPosition => GetOrLoad(ref cameraPositionValue, "ScriptableObjects/CameraPosition");

    private static Vector3Variable cameraRightValue;
    public static Vector3Variable cameraRight => GetOrLoad(ref cameraRightValue, "ScriptableObjects/CameraRight");

    private static BooleanVariable playerInfoCardVisibleStateValue;
    public static BooleanVariable playerInfoCardVisibleState => GetOrLoad(ref playerInfoCardVisibleStateValue, "ScriptableObjects/PlayerInfoCardVisibleState");

    public static RendererState rendererState => GetOrLoad(ref rendererStateValue, "ScriptableObjects/RendererState");
    private static RendererState rendererStateValue;

    private static ReadMessagesDictionary lastReadChatMessagesDictionary;
    public static ReadMessagesDictionary lastReadChatMessages => GetOrLoad(ref lastReadChatMessagesDictionary, "ScriptableObjects/LastReadChatMessages");

    private static LongVariable lastReadChatMessagesValue;
    public static LongVariable lastReadWorldChatMessages => GetOrLoad(ref lastReadChatMessagesValue, "ScriptableObjects/LastReadWorldChatMessages");

    private static BooleanVariable allUIHiddenValue;
    public static BooleanVariable allUIHidden => GetOrLoad(ref allUIHiddenValue, "ScriptableObjects/AllUIHidden");

    private static LatestOpenChatsList latestOpenChatsValue;
    public static LatestOpenChatsList latestOpenChats => GetOrLoad(ref latestOpenChatsValue, "ScriptableObjects/LatestOpenChats");

    internal static T GetOrLoad<T>(ref T variable, string path) where T : Object
    {
        if (variable == null)
        {
            variable = Resources.Load<T>(path);
        }

        return variable;
    }
}
