using DCL;
using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// Helper class that wraps the processing of inputs and modifiers to trigger actions events
/// </summary>
public static class InputProcessor
{
    private static readonly KeyCode[] MODIFIER_KEYS = { KeyCode.LeftControl, KeyCode.LeftAlt, KeyCode.LeftShift, KeyCode.LeftCommand, KeyCode.B };

    [Flags]
    public enum Modifier
    {
        //Set the values as bit masks
        None = 0b0000000, // No modifier needed
        NeedsPointerLocked = 0b0000001, // The pointer must be locked to the game
        FocusNotInInput = 0b0000010, // The game focus cannot be in an input field
        NotInStartMenu = 0b0000100, // The game focus cannot be in full-screen start menu
        OnlyWithInputFocused = 0b0001000, // The game focus must be in an input field
    }

    /// <summary>
    /// Check if the modifier keys are pressed
    /// </summary>
    /// <param name="modifierKeys"> Keycodes modifiers</param>
    /// <returns></returns>
    private static bool PassModifierKeys(KeyCode[] modifierKeys)
    {
        for (var i = 0; i < MODIFIER_KEYS.Length; i++)
        {
            var keyCode = MODIFIER_KEYS[i];
            bool pressed = Input.GetKey(keyCode);

            if (modifierKeys == null)
            {
                if (pressed)
                    return false;
            }
            else
            {
                var anyModifierKeysActive = false;

                foreach (var key in modifierKeys)
                {
                    if (key == keyCode)
                    {
                        anyModifierKeysActive = true;
                        break;
                    }
                }

                if (anyModifierKeysActive != pressed)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if a miscellaneous modifiers are present. These modifiers are related to the meta-state of the application
    /// they can be anything such as mouse pointer state, where the focus is, camera mode...
    /// </summary>
    /// <param name="modifiers"></param>
    /// <returns></returns>
    private static bool PassModifiers(Modifier modifiers)
    {
        if (IsModifierSet(modifiers, Modifier.NeedsPointerLocked) && !Utils.IsCursorLocked)
            return false;

        bool isInputFieldFocused = FocusIsInInputField();

        if (IsModifierSet(modifiers, Modifier.FocusNotInInput) && isInputFieldFocused)
            return false;

        if (IsModifierSet(modifiers, Modifier.OnlyWithInputFocused) && !isInputFieldFocused)
            return false;

        if (IsModifierSet(modifiers, Modifier.NotInStartMenu) && IsStartMenuVisible())
            return false;

        return true;
    }

    private static bool IsStartMenuVisible() => DataStore.i.exploreV2.isOpen.Get();

    /// <summary>
    /// Process an input action mapped to a keyboard key.
    /// </summary>
    /// <param name="action">Trigger Action to perform</param>
    /// <param name="key">KeyCode mapped to this action</param>
    /// <param name="modifierKeys">KeyCodes required to perform the action</param>
    /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    public static void FromKey(InputAction_Trigger action, KeyCode key, KeyCode[] modifierKeys = null, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
            return;

        if (!PassModifierKeys(modifierKeys))
            return;

        if (Input.GetKeyDown(key))
            action.RaiseOnTriggered();
    }

    /// <summary>
    /// Process an input action mapped to a button.
    /// </summary>
    /// <param name="action">Trigger Action to perform</param>
    /// <param name="mouseButtonIdx">Index of the mouse button mapped to this action</param>
    /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    public static void FromMouseButton(InputAction_Trigger action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
            return;

        if (Input.GetMouseButton(mouseButtonIdx))
            action.RaiseOnTriggered();
    }

    public static void FromMouseButtonUp(InputAction_Trigger action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
            return;

        if (Input.GetMouseButtonUp(mouseButtonIdx))
            action.RaiseOnTriggered();
    }

    /// <summary>
    /// Process an input action mapped to a keyboard key
    /// </summary>
    /// <param name="action">Hold Action to perform</param>
    /// <param name="key">KeyCode mapped to this action</param>
    /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    public static void FromKey(InputAction_Hold action, KeyCode key, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
            return;

        if (Input.GetKeyDown(key))
            action.RaiseOnStarted();
        if (Input.GetKeyUp(key))
            action.RaiseOnFinished();
    }

    /// <summary>
    /// Process an input action mapped to a keyboard key
    /// </summary>
    /// <param name="action">Hold Action to perform</param>
    /// <param name="key">KeyCode mapped to this action</param>
    /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    /// <param name="modifierKeys">KeyCodes required to perform the action</param>
    public static void FromKey(InputAction_Hold action, KeyCode key, Modifier modifiers, KeyCode[] modifierKeys)
    {
        if (!PassModifierKeys(modifierKeys))
            return;

        FromKey(action, key, modifiers);
    }

    /// <summary>
    /// Process an input action mapped to a mouse button
    /// </summary>
    /// <param name="action">Hold Action to perform</param>
    /// <param name="mouseButtonIdx">Index of the mouse button</param>
    /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    public static void FromMouse(InputAction_Hold action, int mouseButtonIdx, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
            return;

        if (Input.GetMouseButtonDown(mouseButtonIdx))
            action.RaiseOnStarted();
        if (Input.GetMouseButtonUp(mouseButtonIdx))
            action.RaiseOnFinished();
    }

    /// <summary>
    /// Process an input action mapped to an axis
    /// </summary>
    /// <param name="action">Measurable Action to perform</param>
    /// <param name="axisName">Axis name</param>
    /// <param name="modifiers">Miscellaneous modifiers required for this action</param>
    public static void FromAxis(InputAction_Measurable action, string axisName, Modifier modifiers = Modifier.None)
    {
        if (!PassModifiers(modifiers))
        {
            action.RaiseOnValueChanged(0);
            return;
        }

        action.RaiseOnValueChanged(Input.GetAxis(axisName));
    }

    /// <summary>
    /// Bitwise check for the modifiers flags
    /// </summary>
    /// <param name="modifiers">Modifier to check</param>
    /// <param name="value">Modifier mapped to a bit to check</param>
    /// <returns></returns>
    private static bool IsModifierSet(Modifier modifiers, Modifier value)
    {
        var flagsValue = (int)modifiers;
        var flagValue = (int)value;

        return (flagsValue & flagValue) != 0;
    }

    public static bool FocusIsInInputField()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.currentSelectedGameObject != null &&
               (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null ||
                FocusIsInTextField(EventSystem.current.currentSelectedGameObject));
    }

    /// <summary>
    /// Checks VisualElement from UI Toolkit
    /// </summary>
    private static bool FocusIsInTextField(GameObject currentSelectedObject) =>
        currentSelectedObject.GetComponent<PanelEventHandler>()?.panel?.focusController?.focusedElement is TextField;
}
