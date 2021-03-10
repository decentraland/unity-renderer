using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;

internal class EmailPromptHUDView : MonoBehaviour
{
    public event Action<string> OnSendEmail;
    public event Action<bool> OnDismiss;

    public TMP_InputField inputField;
    public Button_OnPointerDown closeButton;
    public Button_OnPointerDown sendButton;
    public Toggle dontAskAgain;
    public ShowHideAnimator showHideAnimator;
    public GameObject invalidEmailIndicator;
    [SerializeField] internal InputAction_Trigger closeAction;

    private InputAction_Trigger.Triggered closeActionDelegate;

    // NOTE: regex based in https://github.com/Microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs
    const string emailPattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
    const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
    Regex emailRegex = new Regex(emailPattern, options);

    void Awake()
    {
        sendButton.interactable = false;
        invalidEmailIndicator.SetActive(false);

        closeActionDelegate = (x) => OnDismiss?.Invoke(dontAskAgain.isOn);
        closeAction.OnTriggered += closeActionDelegate;

        sendButton.onClick.AddListener(() => OnSendEmail?.Invoke(inputField.text));
        closeButton.onClick.AddListener(() => OnDismiss?.Invoke(dontAskAgain.isOn));

        inputField.onValueChanged.AddListener(value =>
        {
            bool isValidValue = IsValidEmail(value);
            sendButton.interactable = isValidValue;

            if (!string.IsNullOrEmpty(value))
            {
                invalidEmailIndicator.SetActive(!isValidValue);
            }
            else
            {
                invalidEmailIndicator.SetActive(false);
            }
        });

        inputField.onSubmit.AddListener(value =>
        {
            if (sendButton.interactable)
            {
                sendButton.onClick.Invoke();
            }
        });

        showHideAnimator.OnWillFinishStart += OnWillFinishStart;
    }

    void OnWillFinishStart(ShowHideAnimator animator)
    {
        inputField.Select();
        inputField.ActivateInputField();
    }

    void OnDestroy()
    {
        closeAction.OnTriggered -= closeActionDelegate;
        showHideAnimator.OnWillFinishStart -= OnWillFinishStart;
    }

    bool IsValidEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }
}
