using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum DCLAction_Trigger
{
    //Remember to explicitly assign the value to each entry so we minimize issues with serialization + conflicts
    CameraChange = 100,
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
    public InputAction_Trigger[] triggerTimeActions;
    public InputAction_Hold[] holdActions;
    public InputAction_Measurable[] measurableActions;

    private void Update()
    {
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
                    //InputProcessor.FromKey( action, KeyCode.V, InputProcessor.Modifier.NeedsPointerLocked);
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
                    InputProcessor.FromKey( action, KeyCode.LeftShift, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Hold.Jump:
                    InputProcessor.FromKey( action, KeyCode.Space, InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Hold.FreeCameraMode:
                    //Disable until the fine-tuning is ready
                    //InputProcessor.FromKey( action, KeyCode.T, InputProcessor.Modifier.NeedsPointerLocked);
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
                    InputProcessor.FromAxis( action, "Horizontal", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CharacterYAxis:
                    InputProcessor.FromAxis( action, "Vertical", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraXAxis:
                    InputProcessor.FromAxis( action, "Mouse X", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                case DCLAction_Measurable.CameraYAxis:
                    InputProcessor.FromAxis( action, "Mouse Y", InputProcessor.Modifier.NeedsPointerLocked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public static class InputProcessor
{
    [Flags]
    public enum Modifier
    {
        //Set the values as bit masks
        None = 0b0000000,
        NeedsPointerLocked = 0b0000001,
    }

    public static bool PassModifiers(Modifier modifiers)
    {
        bool result = true;

        if (IsModifierSet(modifiers, Modifier.NeedsPointerLocked) && Cursor.lockState != CursorLockMode.Locked)
            return false;

        return true;
    }

    public static void FromKey(InputAction_Trigger action, KeyCode key, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers)) return;

        if (Input.GetKeyDown(key)) action.RaiseOnTriggered();
    }

    public static void FromMouseButton(InputAction_Trigger action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
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
}