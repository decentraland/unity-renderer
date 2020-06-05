using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum DCLAction_Trigger
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CameraChange = 100,

    ToggleNavMap = 110,
    ToggleFriends = 120,
    CloseWindow = 121,
    ToggleWorldChat = 122,
    ToggleUIVisibility = 123,

    OpenExpressions = 200,
    Expression_Wave = 201,
    Expression_FistPump = 202,
    Expression_Robot = 203,
    Expression_RaiseHand = 204,
    Expression_Clap = 205,
    Expression_ThrowMoney = 206,
    Expression_SendKiss = 207
}

public enum DCLAction_Hold
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    Sprint = 1,
    Jump = 2,
    FreeCameraMode = 101,
}

public enum DCLAction_Measurable
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CharacterXAxis = 1,
    CharacterYAxis = 2,
    CameraXAxis = 3,
    CameraYAxis = 4,
}

public class InputController : MonoBehaviour
{
    public static bool ENABLE_THIRD_PERSON_CAMERA = true;

    public InputAction_Trigger[] triggerTimeActions;
    public InputAction_Hold[] holdActions;
    public InputAction_Measurable[] measurableActions;
    bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
    bool allUIHidden => CommonScriptableObjects.allUIHidden.Get();

    private void Update()
    {
        if (!renderingEnabled) return;

        Update_Trigger();
        Update_Hold();
        Update_Measurable();
    }

    public void Update_Trigger()
    {
        for (var i = 0; i < triggerTimeActions.Length; i++)
        {
            var action = triggerTimeActions[i];
            switch (action.GetDCLAction())
            {
                case DCLAction_Trigger.CameraChange:
                    //Disable until the fine-tuning is ready
                    if (ENABLE_THIRD_PERSON_CAMERA)
                        InputProcessor.FromKey(action, KeyCode.V,
                            modifiers: InputProcessor.Modifier.NeedsPointerLocked |
                                       InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleNavMap:
                    if (allUIHidden) break;
                    InputProcessor.FromKey(action, KeyCode.M, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    InputProcessor.FromKey(action, KeyCode.Tab, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.ToggleFriends:
                    if (allUIHidden) break;
                    InputProcessor.FromKey(action, KeyCode.L, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.ToggleWorldChat:
                    if (allUIHidden) break;
                    InputProcessor.FromKey(action, KeyCode.Return, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.ToggleUIVisibility:
                    InputProcessor.FromKey(action, KeyCode.U, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.CloseWindow:
                    if (allUIHidden) break;
                    InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.None);
                    break;
                case DCLAction_Trigger.OpenExpressions:
                    if (allUIHidden) break;
                    InputProcessor.FromKey(action, KeyCode.B, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    InputProcessor.FromKey(action, KeyCode.Escape, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_Wave:
                    InputProcessor.FromKey(action, KeyCode.Alpha1, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_FistPump:
                    InputProcessor.FromKey(action, KeyCode.Alpha2, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_Robot:
                    InputProcessor.FromKey(action, KeyCode.Alpha3, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_RaiseHand:
                    InputProcessor.FromKey(action, KeyCode.Alpha4, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_Clap:
                    InputProcessor.FromKey(action, KeyCode.Alpha5, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_ThrowMoney:
                    InputProcessor.FromKey(action, KeyCode.Alpha6, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                case DCLAction_Trigger.Expression_SendKiss:
                    InputProcessor.FromKey(action, KeyCode.Alpha7, modifiers: InputProcessor.Modifier.FocusNotInInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Update_Hold()
    {
        for (var i = 0; i < holdActions.Length; i++)
        {
            var action = holdActions[i];
            switch (action.GetDCLAction())
            {
                case DCLAction_Hold.Sprint:
                    InputProcessor.FromKey(action, KeyCode.LeftShift, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Hold.Jump:
                    InputProcessor.FromKey(action, KeyCode.Space, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Hold.FreeCameraMode:
                    //Disable until the fine-tuning is ready
                    if (ENABLE_THIRD_PERSON_CAMERA)
                        InputProcessor.FromKey(action, KeyCode.T, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Update_Measurable()
    {
        for (var i = 0; i < measurableActions.Length; i++)
        {
            var action = measurableActions[i];
            switch (action.GetDCLAction())
            {
                case DCLAction_Measurable.CharacterXAxis:
                    InputProcessor.FromAxis(action, "Horizontal", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CharacterYAxis:
                    InputProcessor.FromAxis(action, "Vertical", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraXAxis:
                    InputProcessor.FromAxis(action, "Mouse X", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraYAxis:
                    InputProcessor.FromAxis(action, "Mouse Y", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public static class InputProcessor
{
    private static readonly KeyCode[] MODIFIER_KEYS = new[] { KeyCode.LeftControl, KeyCode.LeftAlt, KeyCode.LeftShift };

    [Flags]
    public enum Modifier
    {
        //Set the values as bit masks
        None = 0b0000000,
        NeedsPointerLocked = 0b0000001,
        FocusNotInInput = 0b0000010,
    }

    public static bool PassModifiers(Modifier modifiers)
    {
        if (IsModifierSet(modifiers, Modifier.NeedsPointerLocked) && !DCL.Helpers.Utils.isCursorLocked)
            return false;

        if (IsModifierSet(modifiers, Modifier.FocusNotInInput) && FocusIsInInputField())
            return false;

        return true;
    }

    public static void FromKey(InputAction_Trigger action, KeyCode key, KeyCode[] modifierKeys = null,
        Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        for (var i = 0; i < MODIFIER_KEYS.Length; i++)
        {
            var keyCode = MODIFIER_KEYS[i];
            var pressed = Input.GetKey(keyCode);
            if (modifierKeys == null)
            {
                if (pressed) return;
            }
            else
            {
                if (modifierKeys.Contains(keyCode) != pressed) return;
            }
        }

        if (Input.GetKeyDown(key)) action.RaiseOnTriggered();
    }

    public static void FromMouseButton(InputAction_Trigger action, int mouseButtonIdx,
        Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetMouseButton(mouseButtonIdx)) action.RaiseOnTriggered();
    }

    public static void FromKey(InputAction_Hold action, KeyCode key, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetKeyDown(key)) action.RaiseOnStarted();
        if (Input.GetKeyUp(key)) action.RaiseOnFinished();
    }

    public static void FromMouse(InputAction_Hold action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetMouseButtonDown(mouseButtonIdx)) action.RaiseOnStarted();
        if (Input.GetMouseButtonUp(mouseButtonIdx)) action.RaiseOnFinished();
    }

    public static void FromAxis(InputAction_Measurable action, string axisName, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
        {
            action.RaiseOnValueChanged(0);
            return;
        }

        action.RaiseOnValueChanged(Input.GetAxis(axisName));
    }

    public static bool IsModifierSet(Modifier modifiers, Modifier value)
    {
        int flagsValue = (int)modifiers;
        int flagValue = (int)value;

        return (flagsValue & flagValue) != 0;
    }

    public static bool FocusIsInInputField()
    {
        if (EventSystem.current.currentSelectedGameObject != null &&
            (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null ||
             EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null))
        {
            return true;
        }

        return false;
    }
}