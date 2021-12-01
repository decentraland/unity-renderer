using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DCL.Builder
{
    public class NewProjectFirstStepView : BaseComponentView
    {
        public event Action OnBackPressed;
        public event Action<string, string> OnNextPressed;

        [SerializeField] internal LimitInputField titleInputField;
        [SerializeField] private LimitInputField descriptionInputField;

        [SerializeField] internal ButtonComponentView nextButton;
        [SerializeField] private ButtonComponentView backButton;

        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();

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

        internal void NextPressed()
        {
            if (!nextButton.IsInteractable())
                return;

            string title = titleInputField.GetValue();
            string description = descriptionInputField.GetValue();

            OnNextPressed?.Invoke(title, description);
        }

        internal void BackPressed() { OnBackPressed?.Invoke(); }

        internal void EnableNextButton() { nextButton.SetInteractable(true); }

        internal void DisableNextButton() { nextButton.SetInteractable(false); }

    }
}