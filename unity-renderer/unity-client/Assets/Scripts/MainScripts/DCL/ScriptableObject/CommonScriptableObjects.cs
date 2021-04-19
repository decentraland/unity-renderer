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

public static class AudioScriptableObjects
{
    // UI events

    private static AudioEvent buttonHoverEvent;
    public static AudioEvent buttonHover => CommonScriptableObjects.GetOrLoad(ref buttonHoverEvent, "ScriptableObjects/AudioEvents/HUDCommon/ButtonHover");

    private static AudioEvent buttonClickEvent;
    public static AudioEvent buttonClick => CommonScriptableObjects.GetOrLoad(ref buttonClickEvent, "ScriptableObjects/AudioEvents/HUDCommon/ButtonClick");

    private static AudioEvent buttonReleaseEvent;
    public static AudioEvent buttonRelease => CommonScriptableObjects.GetOrLoad(ref buttonReleaseEvent, "ScriptableObjects/AudioEvents/HUDCommon/ButtonRelease");

    private static AudioEvent dialogOpenEvent;
    public static AudioEvent dialogOpen => CommonScriptableObjects.GetOrLoad(ref dialogOpenEvent, "ScriptableObjects/AudioEvents/HUDCommon/DialogOpen");

    private static AudioEvent dialogCloseEvent;
    public static AudioEvent dialogClose => CommonScriptableObjects.GetOrLoad(ref dialogCloseEvent, "ScriptableObjects/AudioEvents/HUDCommon/DialogClose");

    private static AudioEvent enableEvent;
    public static AudioEvent enable => CommonScriptableObjects.GetOrLoad(ref enableEvent, "ScriptableObjects/AudioEvents/HUDCommon/Enable");

    private static AudioEvent disableEvent;
    public static AudioEvent disable => CommonScriptableObjects.GetOrLoad(ref disableEvent, "ScriptableObjects/AudioEvents/HUDCommon/Disable");

    private static AudioEvent fadeInEvent;
    public static AudioEvent fadeIn => CommonScriptableObjects.GetOrLoad(ref fadeInEvent, "ScriptableObjects/AudioEvents/HUDCommon/FadeIn");

    private static AudioEvent fadeOutEvent;
    public static AudioEvent fadeOut => CommonScriptableObjects.GetOrLoad(ref fadeOutEvent, "ScriptableObjects/AudioEvents/HUDCommon/FadeOut");

    private static AudioEvent_WithPitchIncrement listItemAppearEvent;
    public static AudioEvent_WithPitchIncrement listItemAppear => CommonScriptableObjects.GetOrLoad(ref listItemAppearEvent, "ScriptableObjects/AudioEvents/HUDCommon/ListItemAppear");

    private static AudioEvent chatReceiveGlobalEvent;
    public static AudioEvent chatReceiveGlobal => CommonScriptableObjects.GetOrLoad(ref chatReceiveGlobalEvent, "ScriptableObjects/AudioEvents/HUDCommon/ChatReceiveGlobal");

    private static AudioEvent chatReceivePrivateEvent;
    public static AudioEvent chatReceivePrivate => CommonScriptableObjects.GetOrLoad(ref chatReceivePrivateEvent, "ScriptableObjects/AudioEvents/HUDCommon/ChatReceivePrivate");

    private static AudioEvent chatSendEvent;
    public static AudioEvent chatSend => CommonScriptableObjects.GetOrLoad(ref chatSendEvent, "ScriptableObjects/AudioEvents/HUDCommon/ChatSend");

    private static AudioEvent notificationEvent;
    public static AudioEvent notification => CommonScriptableObjects.GetOrLoad(ref notificationEvent, "ScriptableObjects/AudioEvents/HUDCommon/Notification");

    private static AudioEvent sliderValueChangeEvent;
    public static AudioEvent sliderValueChange => CommonScriptableObjects.GetOrLoad(ref sliderValueChangeEvent, "ScriptableObjects/AudioEvents/HUDCommon/SliderValueChange");

    private static AudioEvent inputFieldFocusEvent;
    public static AudioEvent inputFieldFocus => CommonScriptableObjects.GetOrLoad(ref inputFieldFocusEvent, "ScriptableObjects/AudioEvents/HUDCommon/InputFieldFocus");

    private static AudioEvent inputFieldUnfocusEvent;
    public static AudioEvent inputFieldUnfocus => CommonScriptableObjects.GetOrLoad(ref inputFieldUnfocusEvent, "ScriptableObjects/AudioEvents/HUDCommon/InputFieldUnfocus");

    private static AudioEvent UIHideEvent;
    public static AudioEvent UIHide => CommonScriptableObjects.GetOrLoad(ref UIHideEvent, "ScriptableObjects/AudioEvents/HUDCommon/UIHide");

    private static AudioEvent UIShowEvent;
    public static AudioEvent UIShow => CommonScriptableObjects.GetOrLoad(ref UIShowEvent, "ScriptableObjects/AudioEvents/HUDCommon/UIUnhide");
}

public static class CommonScriptableObjects
{
    private static Vector3Variable playerUnityPositionValue;
    public static Vector3Variable playerUnityPosition => GetOrLoad(ref playerUnityPositionValue, "ScriptableObjects/PlayerUnityPosition");

    private static Vector3Variable playerWorldPositionValue;
    public static Vector3Variable playerWorldPosition => GetOrLoad(ref playerWorldPositionValue, "ScriptableObjects/PlayerWorldPosition");

    private static Vector3Variable playerUnityEulerAnglesValue;
    public static Vector3Variable playerUnityEulerAngles => GetOrLoad(ref playerUnityEulerAnglesValue, "ScriptableObjects/PlayerUnityEulerAngles");

    private static Vector3Variable worldOffsetValue;
    public static Vector3Variable worldOffset => GetOrLoad(ref worldOffsetValue, "ScriptableObjects/WorldOffset");

    private static Vector2IntVariable playerCoordsValue;
    public static Vector2IntVariable playerCoords => GetOrLoad(ref playerCoordsValue, "ScriptableObjects/PlayerCoords");

    private static BooleanVariable playerIsOnMovingPlatformValue;
    public static BooleanVariable playerIsOnMovingPlatform => GetOrLoad(ref playerIsOnMovingPlatformValue, "ScriptableObjects/playerIsOnMovingPlatform");

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

    private static BooleanVariable cameraIsBlendingValue;
    public static BooleanVariable cameraIsBlending => GetOrLoad(ref cameraIsBlendingValue, "ScriptableObjects/CameraIsBlending");

    private static BooleanVariable cameraBlockedValue;
    public static BooleanVariable cameraBlocked => GetOrLoad(ref cameraBlockedValue, "ScriptableObjects/CameraBlocked");

    private static BooleanVariable playerInfoCardVisibleStateValue;
    public static BooleanVariable playerInfoCardVisibleState => GetOrLoad(ref playerInfoCardVisibleStateValue, "ScriptableObjects/PlayerInfoCardVisibleState");

    public static RendererState rendererState => GetOrLoad(ref rendererStateValue, "ScriptableObjects/RendererState");
    private static RendererState rendererStateValue;

    public static BooleanVariable focusState => GetOrLoad(ref focusStateValue, "ScriptableObjects/FocusState");
    private static BooleanVariable focusStateValue;


    private static ReadMessagesDictionary lastReadChatMessagesDictionary;
    public static ReadMessagesDictionary lastReadChatMessages => GetOrLoad(ref lastReadChatMessagesDictionary, "ScriptableObjects/LastReadChatMessages");

    private static LongVariable lastReadChatMessagesValue;
    public static LongVariable lastReadWorldChatMessages => GetOrLoad(ref lastReadChatMessagesValue, "ScriptableObjects/LastReadWorldChatMessages");

    private static BooleanVariable allUIHiddenValue;
    public static BooleanVariable allUIHidden => GetOrLoad(ref allUIHiddenValue, "ScriptableObjects/AllUIHidden");

    private static BooleanVariable builderInWorldNotNecessaryUIVisibilityStatusValue;
    public static BooleanVariable builderInWorldNotNecessaryUIVisibilityStatus => GetOrLoad(ref builderInWorldNotNecessaryUIVisibilityStatusValue, "ScriptableObjects/BuilderInWorldUIHidden");

    private static LatestOpenChatsList latestOpenChatsValue;
    public static LatestOpenChatsList latestOpenChats => GetOrLoad(ref latestOpenChatsValue, "ScriptableObjects/LatestOpenChats");

    private static CameraMode cameraModeValue;
    public static CameraMode cameraMode => GetOrLoad(ref cameraModeValue, "ScriptableObjects/CameraMode");

    private static BooleanVariable isProfileHUDOpenValue;
    public static BooleanVariable isProfileHUDOpen => GetOrLoad(ref isProfileHUDOpenValue, "ScriptableObjects/IsProfileHUDOpen");

    private static BooleanVariable isFullscreenHUDOpenValue;
    public static BooleanVariable isFullscreenHUDOpen => GetOrLoad(ref isFullscreenHUDOpenValue, "ScriptableObjects/IsAvatarHUDOpen");


    private static BooleanVariable isTaskbarHUDInitializedValue;
    public static BooleanVariable isTaskbarHUDInitialized => GetOrLoad(ref isTaskbarHUDInitializedValue, "ScriptableObjects/IsTaskbarHUDInitialized");

    private static BooleanVariable tutorialActiveValue;
    public static BooleanVariable tutorialActive => GetOrLoad(ref tutorialActiveValue, "ScriptableObjects/TutorialActive");

    private static BooleanVariable featureKeyTriggersBlockedValue;
    public static BooleanVariable featureKeyTriggersBlocked => GetOrLoad(ref featureKeyTriggersBlockedValue, "ScriptableObjects/FeatureKeyTriggersBlocked");

    private static BooleanVariable motdActiveValue;
    public static BooleanVariable motdActive => GetOrLoad(ref motdActiveValue, "ScriptableObjects/MOTDActive");

    private static BooleanVariable emailPromptActiveValue;
    public static BooleanVariable emailPromptActive => GetOrLoad(ref emailPromptActiveValue, "ScriptableObjects/EmailPromptActive");

    private static BooleanVariable voiceChatDisabledValue;
    public static BooleanVariable voiceChatDisabled => GetOrLoad(ref voiceChatDisabledValue, "ScriptableObjects/VoiceChatDisabled");

    public static T GetOrLoad<T>(ref T variable, string path) where T : Object
    {
        if (variable == null)
        {
            variable = Resources.Load<T>(path);
        }

        return variable;
    }
}