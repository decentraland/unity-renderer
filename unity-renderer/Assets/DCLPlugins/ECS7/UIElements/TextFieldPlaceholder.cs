﻿using DCL.ECSComponents;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.UIElements
{
    /// <summary>
    /// Adds placeholder to the `TextField`;
    /// In Unity 2023 `Placeholder` is included in the framework
    /// </summary>
    public class TextFieldPlaceholder
    {
        private string placeholder;
        private Color placeholderColor;

        private Color normalColor;

        private readonly TextField textField;

        private bool isPlaceholder;
        private bool isFocused;

        public TextFieldPlaceholder(TextField textField)
        {
            this.textField = textField;

            textField.RegisterCallback<FocusInEvent>(OnFocusIn);
            textField.RegisterCallback<FocusOutEvent>(OnFocusOut);

            OnFocusOut(null);
        }

        public void SetPlaceholder(string placeholder)
        {
            this.placeholder = placeholder;
            UpdateIfFocusStateIs(false);
        }

        public void SetNormalColor(Color4 color)
        {
            normalColor = color.ToUnityColor();
            UpdateIfFocusStateIs(true);
        }

        public void SetPlaceholderColor(Color4 color)
        {
            placeholderColor = color.ToUnityColor();
            UpdateIfFocusStateIs(false);
        }

        private void UpdateIfFocusStateIs(bool focusState)
        {
            if (focusState != isFocused)
                return;

            if (focusState)
            {
                SetNormalStyle();
            }
            else
            {
                SetPlaceholderStyle();
                textField.SetValueWithoutNotify(placeholder);
            }
        }

        private void SetNormalStyle()
        {
            textField.style.color = normalColor;
        }

        private void SetPlaceholderStyle()
        {
            textField.style.color = placeholderColor;
        }

        private void OnFocusIn(FocusInEvent _)
        {
            if (isPlaceholder)
            {
                textField.SetValueWithoutNotify(string.Empty);
                SetNormalStyle();
            }

            isFocused = true;
        }

        private void OnFocusOut(FocusOutEvent _)
        {
            if (string.IsNullOrEmpty(textField.text))
            {
                textField.SetValueWithoutNotify(placeholder);
                SetPlaceholderStyle();
                isPlaceholder = true;
            }

            isFocused = false;
        }
    }
}
