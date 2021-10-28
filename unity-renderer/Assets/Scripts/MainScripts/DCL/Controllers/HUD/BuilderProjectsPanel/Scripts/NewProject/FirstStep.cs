using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FirstStep : BaseComponentView
{
    public event Action OnBackPressed;
    public event Action<string, string> OnNextPressed;

    [SerializeField] private LimitInputField titleInputField;
    [SerializeField] private LimitInputField descriptionInputField;

    [SerializeField] private ButtonComponentView nextButton;
    [SerializeField] private ButtonComponentView backButton;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        titleInputField.OnLimitReached += DisableNextButton;
        titleInputField.OnEmptyValue += DisableNextButton;
        titleInputField.OnInputAvailable += EnableNextButton;

        descriptionInputField.OnLimitReached += DisableNextButton;
        descriptionInputField.OnInputAvailable += EnableNextButton;

        backButton.onClick.AddListener(BackPressed);

        nextButton.onClick.AddListener(NextPressed);

        DisableNextButton();
    }

    public override void RefreshControl() { }

    public override void Dispose()
    {
        base.Dispose();
        titleInputField.OnLimitReached -= DisableNextButton;
        titleInputField.OnEmptyValue -= DisableNextButton;
        titleInputField.OnInputAvailable -= EnableNextButton;

        descriptionInputField.OnLimitReached -= DisableNextButton;
        descriptionInputField.OnInputAvailable -= EnableNextButton;

        backButton.onClick.RemoveListener(BackPressed);
        nextButton.onClick.RemoveListener(NextPressed);
    }

    private void NextPressed()
    {
        if (!nextButton.IsInteractable())
            return;

        string title = titleInputField.GetValue();
        string description = descriptionInputField.GetValue();

        OnNextPressed?.Invoke(title, description);
    }

    private void BackPressed() { OnBackPressed?.Invoke(); }

    private void EnableNextButton() { nextButton.SetInteractable(true); }

    private void DisableNextButton() { nextButton.SetInteractable(false); }

}