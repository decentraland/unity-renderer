using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LimitInputField : MonoBehaviour
{
    public event System.Action OnLimitReached;
    public event System.Action OnEmptyValue;
    public event System.Action OnInputAvailable;

    public event System.Action<string> OnInputChange;

    [SerializeField] private int characterLimit;
    [SerializeField] private bool isMandatory = false;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color errorColor;

    [SerializeField] private Sprite inputFieldNormalBackgroundSprite;
    [SerializeField] private Sprite inputFieldErrorBackgroundSprite;
    [SerializeField] private Sprite inputFieldFocusBackgroundSprite;

    [SerializeField] private Image inputFieldbackgroundImg;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI limitText;

    internal bool hasPassedLimit = false;
    internal bool hasBeenEmpty = false;
    private string currentValue = "";

    private void Start()
    {
        inputField.onSelect.AddListener(InputFocused);
        inputField.onDeselect.AddListener(InputLostFocus);
        inputField.onValueChanged.AddListener(InputChanged);
        limitText?.SetText( currentValue.Length + "/" + characterLimit);
        if (isMandatory)
            hasBeenEmpty = true;
    }

    public void SetError() { LimitReached(); }

    private void InputLostFocus(string value)
    {
        if (value.Length > characterLimit)
            inputFieldbackgroundImg.sprite = inputFieldErrorBackgroundSprite;
        else
            inputFieldbackgroundImg.sprite = inputFieldNormalBackgroundSprite;
    }

    private void InputFocused(string value)
    {
        if (value.Length > characterLimit)
            inputFieldbackgroundImg.sprite = inputFieldErrorBackgroundSprite;
        else
            inputFieldbackgroundImg.sprite = inputFieldFocusBackgroundSprite;
    }

    internal void InputChanged(string newValue)
    {
        currentValue = newValue;
        limitText?.SetText(newValue.Length + "/" + characterLimit);
        if (newValue.Length > characterLimit)
            LimitReached();
        else if (currentValue.Length == 0)
            Empty();
        else
            InputAvailable();

        OnInputChange?.Invoke(newValue);
    }

    internal void InputAvailable()
    {
        if (!hasPassedLimit && !hasBeenEmpty)
            return;

        if (limitText != null)
            limitText.color = normalColor;
        inputFieldbackgroundImg.sprite = inputFieldFocusBackgroundSprite;
        OnInputAvailable?.Invoke();
        hasPassedLimit = false;
        hasBeenEmpty = false;
    }

    internal void Empty()
    {
        if (hasBeenEmpty)
            return;

        hasBeenEmpty = true;
        OnEmptyValue?.Invoke();
    }

    internal void LimitReached()
    {
        if (hasPassedLimit)
            return;

        if (limitText != null)
            limitText.color = errorColor;
        inputFieldbackgroundImg.sprite = inputFieldErrorBackgroundSprite;

        OnLimitReached?.Invoke();
        hasPassedLimit = true;
    }

    public string GetValue() { return currentValue; }
}