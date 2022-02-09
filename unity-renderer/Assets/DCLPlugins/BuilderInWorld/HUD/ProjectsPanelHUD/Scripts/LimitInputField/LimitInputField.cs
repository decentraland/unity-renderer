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
    public event System.Action OnInputFocused;

    public event System.Action<string> OnInputChange;

    [SerializeField] private int characterLimit;
    [SerializeField] private bool isMandatory = false;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color errorColor;
    [SerializeField] private Color focusColor;

    [SerializeField] private Image inputFieldbackgroundImg;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI limitText;

    internal bool hasPassedLimit = false;
    internal bool hasBeenEmpty = false;
    internal bool hasFocus = false;
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

    public void SetCharacterLimit(int limit)
    {
        characterLimit = limit;
    }
    
    public void SetText(string value)
    {
        currentValue = value;
        inputField.SetTextWithoutNotify(value);
    }

    public void SetError() { LimitReached(); }

    private void InputLostFocus(string value)
    {
        hasFocus = false;
        
        if (hasPassedLimit)
        {
            inputFieldbackgroundImg.enabled = true;
            inputFieldbackgroundImg.color = errorColor;
        }
        else
        {
            inputFieldbackgroundImg.enabled = false;
        }
    }

    private void InputFocused(string value)
    {
        hasFocus = true;
        
        if (hasPassedLimit)
        {
            inputFieldbackgroundImg.enabled = true;
            inputFieldbackgroundImg.color = errorColor;
        }
        else
        {
            inputFieldbackgroundImg.enabled = false;
        }
        OnInputFocused?.Invoke();
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

    public void InputAvailable()
    {
        if (!hasPassedLimit && !hasBeenEmpty)
            return;

        if (limitText != null)
            limitText.color = normalColor;

        if (hasFocus)
        {
            inputFieldbackgroundImg.enabled = true;
            inputFieldbackgroundImg.color = focusColor;
        }
        else
        {
            inputFieldbackgroundImg.enabled = false;
        }
        
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

    public void LimitReached()
    {
        if (hasPassedLimit)
            return;

        if (limitText != null)
            limitText.color = errorColor;
        inputFieldbackgroundImg.enabled = true;
        inputFieldbackgroundImg.color = errorColor;

        OnLimitReached?.Invoke();
        hasPassedLimit = true;
    }

    public string GetValue() { return currentValue; }
}