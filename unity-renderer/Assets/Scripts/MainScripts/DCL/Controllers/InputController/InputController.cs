using DCL;
using DCL.Configuration;
using System;
using UnityEngine;

/// <summary>
/// Mapping for Trigger actions
/// </summary>
public enum DCLAction_Trigger
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CameraChange = 100,
    CursorUnlock = 101,

    ToggleNavMap = 110,
    ToggleFriends = 120,
    CloseWindow = 121,
    ToggleWorldChat = 122,
    ToggleUIVisibility = 123,
    ToggleControlsHud = 124,
    ToggleSettings = 125,
    ToggleStartMenu = 126,
    ToggleVoiceChatRecording = 127,
    ToggleAvatarEditorHud = 128,
    ToggleQuestsPanelHud = 129,
    ToggleAvatarNamesHud = 130,
    TogglePlacesAndEventsHud = 131,
    ToggleShortcut0 = 132,
    ToggleShortcut1 = 133,
    ToggleShortcut2 = 134,
    ToggleShortcut3 = 135,
    ToggleShortcut4 = 136,
    ToggleShortcut5 = 137,
    ToggleShortcut6 = 138,
    ToggleShortcut7 = 139,
    ToggleShortcut8 = 140,
    ToggleShortcut9 = 141,
    ToggleEmoteShortcut0 = 142,
    ToggleEmoteShortcut1 = 143,
    ToggleEmoteShortcut2 = 144,
    ToggleEmoteShortcut3 = 145,
    ToggleEmoteShortcut4 = 146,
    ToggleEmoteShortcut5 = 147,
    ToggleEmoteShortcut6 = 148,
    ToggleEmoteShortcut7 = 149,
    ToggleEmoteShortcut8 = 150,
    ToggleEmoteShortcut9 = 151,
    ChatPreviousInHistory = 152,
    ChatNextInHistory = 153,
    ChatMentionNextEntry = 154,
    ChatMentionPreviousEntry = 155,

    Expression_Wave = 201,
    Expression_FistPump = 202,
    Expression_Robot = 203,
    Expression_RaiseHand = 204,
    Expression_Clap = 205,
    Expression_ThrowMoney = 206,
    Expression_SendKiss = 207,
    Expression_Dance = 208,
    Expression_Hohoho = 209,
    Expression_Snowfall = 210,
}

/// <summary>
/// Mapping for hold actions
/// </summary>
public enum DCLAction_Hold
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    Sprint = 1,
    Jump = 2,
    ZoomIn = 3,
    ZoomOut = 4,
    FreeCameraMode = 101,
    VoiceChatRecording = 102,
    DefaultConfirmAction = 300,
    DefaultCancelAction = 301,
    OpenExpressions = 447
}

/// <summary>
/// Mapping for measurable actions
/// </summary>
public enum DCLAction_Measurable
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CharacterXAxis = 1,
    CharacterYAxis = 2,
    CameraXAxis = 3,
    CameraYAxis = 4,
    MouseWheel = 5
}

/// <summary>
/// Input Controller will map inputs(keys/mouse/axis) to DCL actions, check if they can be triggered (modifiers) and raise the events
/// </summary>
public class InputController : MonoBehaviour
{
    public static bool ENABLE_THIRD_PERSON_CAMERA = true;

    private readonly KeyCode[] modifierKeyB = { KeyCode.B };
    private readonly KeyCode[] modifierKeyLeftAlt = { KeyCode.LeftAlt };

    [Header("General Input")]
    public InputAction_Trigger[] triggerTimeActions;
    public InputAction_Hold[] holdActions;
    public InputAction_Measurable[] measurableActions;

    bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
    bool allUIHidden => CommonScriptableObjects.allUIHidden.Get();

    private void Update()
    {
        if (!renderingEnabled)
        {
            Stop_Measurable(measurableActions);
            return;
        }

        Update_Trigger(triggerTimeActions);
        Update_Hold(holdActions);
        Update_Measurable(measurableActions);
    }

    /// <summary>
    /// Map the trigger actions to inputs + modifiers and check if their events must be triggered
    /// </summary>
    private void Update_Trigger(InputAction_Trigger[] triggerTimeActions)
    {
        foreach (var action in triggerTimeActions)
        {
            if (action.isTriggerBlocked != null && action.isTriggerBlocked.Get())
                continue;


            switch (action.DCLAction)
            {
                case DCLAction_Trigger.CameraChange:
                    // Disable until the fine-tuning is ready
                    if (!CommonScriptableObjects.cameraModeInputLocked.Get() && ENABLE_THIRD_PERSON_CAMERA)
                        InputProcessor.FromKey(action, KeyCode.V, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.CursorUnlock:
                    InputProcessor.FromMouseButtonUp(action, mouseButtonIdx: 1, modifiers: InputProcessor.Modifier.NeedsPointerLocked);
#if !WEB_PLATFORM
                    InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.NeedsPointerLocked);
#endif
                    break;
                case DCLAction_Trigger.ToggleNavMap:
                    if (!DataStore.i.common.isWorld.Get())
                        InputProcessor.FromKey(action, KeyCode.M, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleFriends:
                    if (!allUIHidden)
                        InputProcessor.FromKey(action, KeyCode.L, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.ToggleWorldChat:
                    if (!allUIHidden)
                        InputProcessor.FromKey(action, KeyCode.Return, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleUIVisibility:
                    InputProcessor.FromKey(action, KeyCode.U, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.CloseWindow:
                    if (!allUIHidden && !DataStore.i.common.isSignUpFlow.Get())
                        InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.ToggleControlsHud:
                    InputProcessor.FromKey(action, KeyCode.C, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleSettings:
                    InputProcessor.FromKey(action, KeyCode.P, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleStartMenu:
                    InputProcessor.FromKey(action, KeyCode.Tab, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.TogglePlacesAndEventsHud:
                    InputProcessor.FromKey(action, KeyCode.X, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut0:
                    InputProcessor.FromKey(action, KeyCode.Alpha0, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut1:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut2:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut3:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut4:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut5:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut6:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut7:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut8:
                    InputProcessor.FromKey(action, KeyCode.Alpha8, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleShortcut9:
                    InputProcessor.FromKey(action, KeyCode.Alpha9, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut0:
                    InputProcessor.FromKey(action, KeyCode.Alpha0, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut1:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut2:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut3:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut4:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut5:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut6:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut7:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut8:
                    InputProcessor.FromKey(action, KeyCode.Alpha8, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ToggleEmoteShortcut9:
                    InputProcessor.FromKey(action, KeyCode.Alpha9, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyB);
                    break;
                case DCLAction_Trigger.ChatNextInHistory:
                    InputProcessor.FromKey(action, KeyCode.UpArrow, modifiers: InputProcessor.Modifier.OnlyWithInputFocused);
                    break;
                case DCLAction_Trigger.ChatPreviousInHistory:
                    InputProcessor.FromKey(action, KeyCode.DownArrow, modifiers: InputProcessor.Modifier.OnlyWithInputFocused);
                    break;
                case DCLAction_Trigger.ChatMentionNextEntry:
                    InputProcessor.FromKey(action, KeyCode.DownArrow, modifiers: InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.ChatMentionPreviousEntry:
                    InputProcessor.FromKey(action, KeyCode.UpArrow, modifiers: InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_Wave:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_FistPump:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_Robot:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_RaiseHand:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_Clap:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_ThrowMoney:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.Expression_SendKiss:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Trigger.ToggleVoiceChatRecording:
                    InputProcessor.FromKey(action, KeyCode.T, modifiers: InputProcessor.Modifier.FocusNotInInput, modifierKeys: modifierKeyLeftAlt);
                    break;
                case DCLAction_Trigger.ToggleAvatarEditorHud:
                    InputProcessor.FromKey(action, KeyCode.I, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleQuestsPanelHud:
                    InputProcessor.FromKey(action, KeyCode.J, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleAvatarNamesHud:
                    InputProcessor.FromKey(action, KeyCode.N, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Map the hold actions to inputs + modifiers and check if their events must be triggered
    /// </summary>
    private void Update_Hold(InputAction_Hold[] holdActions)
    {
        foreach (var action in holdActions)
        {
            if (action.isHoldBlocked != null && action.isHoldBlocked.Get())
                continue;

            switch (action.DCLAction)
            {
                case DCLAction_Hold.Sprint:
                    InputProcessor.FromKey(action, InputSettings.WalkButtonKeyCode,
                        InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Hold.Jump:
                    InputProcessor.FromKey(action, InputSettings.JumpButtonKeyCode,
                        InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Hold.ZoomIn:
                    InputProcessor.FromKey(action, KeyCode.KeypadPlus);
                    InputProcessor.FromKey(action, KeyCode.Plus);
                    break;
                case DCLAction_Hold.ZoomOut:
                    InputProcessor.FromKey(action, KeyCode.KeypadMinus);
                    InputProcessor.FromKey(action, KeyCode.Minus);
                    break;
                case DCLAction_Hold.FreeCameraMode:
                    // Disable until the fine-tuning is ready
                    if (ENABLE_THIRD_PERSON_CAMERA)
                        InputProcessor.FromKey(action, KeyCode.Y, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Hold.VoiceChatRecording:
                    // Push to talk functionality only triggers if no modifier key is pressed
                    InputProcessor.FromKey(action, KeyCode.T, InputProcessor.Modifier.FocusNotInInput, null);
                    break;
                case DCLAction_Hold.DefaultConfirmAction:
                    InputProcessor.FromKey(action, KeyCode.E);
                    break;
                case DCLAction_Hold.DefaultCancelAction:
                    InputProcessor.FromKey(action, KeyCode.F);
                    break;
                case DCLAction_Hold.OpenExpressions:
                    InputProcessor.FromKey(action, KeyCode.B, InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Map the measurable actions to inputs + modifiers and check if their events must be triggered
    /// </summary>
    private void Update_Measurable(InputAction_Measurable[] measurableActions)
    {
        foreach (var action in measurableActions)
        {
            if (action.isMeasurableBlocked != null && action.isMeasurableBlocked.Get())
                continue;

            switch (action.DCLAction)
            {
                case DCLAction_Measurable.CharacterXAxis:
                    InputProcessor.FromAxis(action, "Horizontal", InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Measurable.CharacterYAxis:
                    InputProcessor.FromAxis(action, "Vertical", InputProcessor.Modifier.FocusNotInInput | InputProcessor.Modifier.NotInStartMenu);
                    break;
                case DCLAction_Measurable.CameraXAxis:
                    InputProcessor.FromAxis(action, "Mouse X", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraYAxis:
                    InputProcessor.FromAxis(action, "Mouse Y", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.MouseWheel:
                    InputProcessor.FromAxis(action, "Mouse ScrollWheel", modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void Stop_Measurable(InputAction_Measurable[] measurableActions)
    {
        foreach (var action in measurableActions)
            action.RaiseOnValueChanged(0);
    }
}
