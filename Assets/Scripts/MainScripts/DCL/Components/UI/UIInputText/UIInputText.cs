using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIInputText : UIShape<UIInputTextRefContainer, UIInputText.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public TextShape.Model textModel;
            public string placeholder;
            public Color placeholderColor;
            public Color focusedBackground;

            public string onTextSubmit;
            public string onChanged;
            public string onFocus;
            public string onBlur;
        }

        public override string referencesContainerPrefabName => "UIInputText";

        public TMP_Text tmpText => referencesContainer.text;
        public TMP_InputField inputField => referencesContainer.inputField;
        public RectTransform rectTransform => referencesContainer.rectTransform;

        public UIInputText(ParcelScene scene) : base(scene)
        {
        }


        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): We have to serialize twice now, but in the future we should fix the
            //             client data structure to be like this, so we can serialize all of it in one shot.
            if (!scene.isTestScene)
                model.textModel = JsonUtility.FromJson<TextShape.Model>(newJson);

            inputField.textViewport = referencesContainer.rectTransform;

            UnsuscribeFromEvents();

            inputField.onSelect.AddListener(OnFocus);
            inputField.onDeselect.AddListener(OnBlur);
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.onValueChanged.AddListener(OnChanged);

            DCL.Components.TextShape.ApplyModelChanges(tmpText, model.textModel);

            inputField.text = model.placeholder;
            inputField.textComponent.color = new Color(model.placeholderColor.r, model.placeholderColor.g, model.placeholderColor.b, model.placeholderColor.a);
            referencesContainer.bgImage.color = new Color(model.focusedBackground.r, model.focusedBackground.g, model.focusedBackground.b, model.focusedBackground.a);
            return null;
        }

        public void OnFocus(string call)
        {
            if (inputField.text == model.placeholder)
                inputField.text = "";

            inputField.customCaretColor = true;
            inputField.caretColor = Color.white;
            Interface.WebInterface.ReportOnFocusEvent(scene.sceneData.id, model.onFocus);
        }

        public void OnChanged(string call)
        {
            Interface.WebInterface.ReportOnChangedEvent(scene.sceneData.id, model.onChanged, call, 0);
        }

        public void OnBlur(string call)
        {
            HideCaret();
            Interface.WebInterface.ReportOnBlurEvent(scene.sceneData.id, model.onBlur);
        }

        public void HideCaret()
        {
            if (string.IsNullOrEmpty(inputField.text))
                inputField.text = model.placeholder;

            inputField.customCaretColor = true;
            inputField.caretColor = Color.clear;
            inputField.selectionAnchorPosition = 0;
            inputField.selectionFocusPosition = 0;
            inputField.selectionStringAnchorPosition = 0;
            inputField.selectionStringFocusPosition = 0;
        }

        public void OnSubmit(string call)
        {
            bool validString = !string.IsNullOrEmpty(tmpText.text);

            if (tmpText.text.Length == 1 && (byte)tmpText.text[0] == 11) //NOTE(Brian): Trim doesn't work. neither IsNullOrWhitespace.
                validString = false;

            if (validString)
            {
                Interface.WebInterface.ReportOnTextSubmitEvent(scene.sceneData.id, model.onTextSubmit, tmpText.text);
            }

            inputField.text = "";
            inputField.caretColor = Color.white;
            inputField.Select();
            inputField.ActivateInputField();
        }

        void UnsuscribeFromEvents()
        {
            inputField.onSelect.RemoveAllListeners();
            inputField.onDeselect.RemoveAllListeners();
            inputField.onSubmit.RemoveAllListeners();
            inputField.onValueChanged.RemoveAllListeners();
        }


        public override void Dispose()
        {
            UnsuscribeFromEvents();
            base.Dispose();
        }
    }
}
