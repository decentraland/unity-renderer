using DCL.ECSComponents;
using Decentraland.Common;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.UIElements
{
    /// <summary>
    /// Adds placeholder to the `TextField`;
    /// In Unity 2023 `Placeholder` is included in the framework
    /// </summary>
    public class TextFieldPlaceholder : IDisposable
    {
        private string placeholder;
        private Color placeholderColor;

        private Color normalColor;

        private readonly TextField textField;

        private bool isPlaceholder;
        private bool isFocused;

        private bool isReadonly;

        public TextFieldPlaceholder(TextField textField)
        {
            this.textField = textField;

            textField.RegisterCallback<FocusInEvent>(OnFocusIn);
            textField.RegisterCallback<FocusOutEvent>(OnFocusOut);
            // To support changing the value from code
            textField.RegisterValueChangedCallback(OnValueChanged);

            OnFocusOut(null);
        }

        public void SetPlaceholder(string placeholder)
        {
            this.placeholder = placeholder;

            if (isPlaceholder)
                UpdateIfFocusStateIs(false);
        }

        public void SetNormalColor(Color4 color)
        {
            normalColor = color.ToUnityColor();

            if (!isPlaceholder || isFocused)
                SetNormalStyle();
        }

        public void SetReadOnly(bool isReadOnly)
        {
            this.isReadonly = isReadOnly;
        }

        public void SetPlaceholderColor(Color4 color)
        {
            placeholderColor = color.ToUnityColor();

            if (isPlaceholder)
                UpdateIfFocusStateIs(false);
        }

        private void UpdateIfFocusStateIs(bool focusState)
        {
            if (isFocused != focusState)
                return;

            if (focusState)
            {
                SetNormalStyle();
            }
            else
            {
                SetPlaceholderStyle();
            }
        }

        private void SetNormalStyle()
        {
            textField.style.color = normalColor;
        }

        private void SetPlaceholderStyle()
        {
            textField.style.color = placeholderColor;
            textField.SetValueWithoutNotify(placeholder);
        }

        private void OnFocusIn(FocusInEvent _)
        {
            if (isReadonly)
                return;

            if (isPlaceholder)
            {
                textField.SetValueWithoutNotify(string.Empty);
                SetNormalStyle();
            }

            isFocused = true;
        }

        private void OnFocusOut(FocusOutEvent _)
        {
            if (isReadonly)
                return;

            if (InputIsNullOrEmpty())
            {
                SetPlaceholderStyle();
                isPlaceholder = true;
            }
            else
            {
                SetNormalStyle();
                isPlaceholder = false;
            }

            isFocused = false;
        }

        private bool InputIsNullOrEmpty() =>
            string.IsNullOrEmpty(textField.text);

        private void OnValueChanged(ChangeEvent<string> newValue)
        {
            if (!isFocused)
            {
                OnFocusOut(null);
            }
        }

        public void Dispose()
        {
            textField.UnregisterCallback<FocusInEvent>(OnFocusIn);
            textField.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            textField.UnregisterValueChangedCallback(OnValueChanged);
        }
    }
}
