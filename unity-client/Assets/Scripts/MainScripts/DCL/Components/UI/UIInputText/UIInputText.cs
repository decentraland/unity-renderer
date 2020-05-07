using DCL.Controllers;
using DCL.Models;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL.Components
{
    public class UIInputText : UIShape<UIInputTextRefContainer, UIInputText.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public TextShape.Model textModel;
            public string placeholder;
            public Color placeholderColor = Color.white;
            public Color focusedBackground = Color.black;

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

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIContainerRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): We have to serialize twice now, but in the future we should fix the
            //             client data structure to be like this, so we can serialize all of it in one shot.
            if (!scene.isTestScene)
            {
                model.textModel = SceneController.i.SafeFromJson<TextShape.Model>(newJson);
            }

            inputField.textViewport = referencesContainer.rectTransform;

            UnsuscribeFromEvents();

            inputField.onSelect.AddListener(OnFocus);
            inputField.onDeselect.AddListener(OnBlur);
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.onValueChanged.AddListener(OnChanged);

            yield return DCL.Components.TextShape.ApplyModelChanges(scene, tmpText, model.textModel);

            inputField.text = model.placeholder;
            inputField.textComponent.color = new Color(model.placeholderColor.r, model.placeholderColor.g,
                model.placeholderColor.b, model.placeholderColor.a);
            referencesContainer.bgImage.color = new Color(model.focusedBackground.r, model.focusedBackground.g,
                model.focusedBackground.b, model.focusedBackground.a);
        }

        public void OnFocus(string call)
        {
            if (inputField.text == model.placeholder)
            {
                inputField.text = "";
            }

            inputField.customCaretColor = true;
            inputField.caretColor = Color.white;
            Interface.WebInterface.ReportOnFocusEvent(scene.sceneData.id, model.onFocus);
        }

        public void OnChanged(string changedText)
        {
            Interface.WebInterface.ReportOnTextInputChangedEvent(scene.sceneData.id, model.onChanged, changedText);
        }

        public void OnBlur(string call)
        {
            HideCaret();
            Interface.WebInterface.ReportOnBlurEvent(scene.sceneData.id, model.onBlur);
        }

        public void HideCaret()
        {
            if (string.IsNullOrEmpty(inputField.text))
            {
                inputField.text = model.placeholder;
            }

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
            {
                validString = false;
            }

            if (validString)
            {
                Interface.WebInterface.ReportOnTextSubmitEvent(scene.sceneData.id, model.onTextSubmit, tmpText.text);

                ForceFocus();
            }
            else if(scene.isPersistent) // DCL UI Chat text input
            {
                inputField.DeactivateInputField();
                referencesContainer.mouseCatcher.LockCursor();

                // To avoid focusing the chat in the same frame we unfocused it
                referencesContainer.inputDetectionPausedFrames = 1;
            }
        }

        public void ForceFocus()
        {
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