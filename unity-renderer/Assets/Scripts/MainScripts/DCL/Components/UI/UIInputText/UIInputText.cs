using DCL.Controllers;
using DCL.Models;
using System.Collections;
using DCL.Helpers;
using TMPro;
using UnityEngine;

namespace DCL.Components
{
    public class UIInputText : UIShape<UIInputTextRefContainer, UIInputText.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float outlineWidth = 0f;
            public Color outlineColor = Color.white;

            public Color color = Color.white;

            //These values exist in the SDK but we are doing nothing with these values
            public float thickness = 1f;
            public Color placeHolderColor = Color.white;
            public float maxWidth = 100f;
            public bool autoStretchWidth = true;
            public Color background = Color.black;
            public string fontWeight = "normal";
            //

            public float fontSize = 100f;
            public string font;
            public string value = "";
            public string hTextAlign = "bottom";
            public string vTextAlign = "left";
            public bool textWrapping = false;

            public float shadowBlur = 0f;
            public float shadowOffsetX = 0f;
            public float shadowOffsetY = 0f;
            public Color shadowColor = new Color(1, 1, 1);

            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;

            public string placeholder;
            public Color placeholderColor = Color.white;
            public Color focusedBackground = Color.black;

            public string onTextSubmit;
            public string onChanged;
            public string onFocus;
            public string onBlur;

            public override BaseModel GetDataFromJSON(string json)
            {
                Model model = Utils.SafeFromJson<Model>(json);
                return model;
            }
        }

        public override string referencesContainerPrefabName => "UIInputText";

        public TMP_Text tmpText => referencesContainer.text;
        public TMP_InputField inputField => referencesContainer.inputField;
        public RectTransform rectTransform => referencesContainer.rectTransform;

        public UIInputText() { model = new Model(); }

        public override int GetClassId() { return (int) CLASS_ID.UI_INPUT_TEXT_SHAPE; }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIContainerRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null) { }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            //NOTE(Brian): We have to serialize twice now, but in the future we should fix the
            //             client data structure to be like this, so we can serialize all of it in one shot.
            model = (Model) newModel;

            inputField.textViewport = referencesContainer.rectTransform;

            UnsuscribeFromEvents();

            inputField.onSelect.AddListener(OnFocus);
            inputField.onDeselect.AddListener(OnBlur);
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.onValueChanged.AddListener(OnChanged);

            yield return ApplyModelChanges(scene, tmpText, model);

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

        public void OnChanged(string changedText) { Interface.WebInterface.ReportOnTextInputChangedEvent(scene.sceneData.id, model.onChanged, changedText); }

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

            if (tmpText.text.Length == 1 && (byte) tmpText.text[0] == 11) //NOTE(Brian): Trim doesn't work. neither IsNullOrWhitespace.
            {
                validString = false;
            }

            if (validString)
            {
                Interface.WebInterface.ReportOnTextSubmitEvent(scene.sceneData.id, model.onTextSubmit, tmpText.text);

                ForceFocus();
            }
            else if (scene.isPersistent) // DCL UI Chat text input
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

        private IEnumerator ApplyModelChanges(IParcelScene scene, TMP_Text text, Model model)
        {
            if (!string.IsNullOrEmpty(model.font))
            {
                yield return DCLFont.SetFontFromComponent(scene, model.font, text);
            }

            text.text = model.value;

            text.color = new Color(model.color.r, model.color.g, model.color.b, model.visible ? model.opacity : 0);
            text.fontSize = (int) model.fontSize;
            text.richText = true;
            text.overflowMode = TextOverflowModes.Overflow;

            text.margin =
                new Vector4
                (
                    (int) model.paddingLeft,
                    (int) model.paddingTop,
                    (int) model.paddingRight,
                    (int) model.paddingBottom
                );

            text.alignment = TextShape.GetAlignment(model.vTextAlign, model.hTextAlign);
            text.lineSpacing = 0f;

            text.maxVisibleLines = int.MaxValue;


            text.enableWordWrapping = model.textWrapping && !text.enableAutoSizing;

            if (model.shadowOffsetX != 0 || model.shadowOffsetY != 0)
            {
                text.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
                text.fontSharedMaterial.SetColor("_UnderlayColor", model.shadowColor);
                text.fontSharedMaterial.SetFloat("_UnderlaySoftness", model.shadowBlur);
            }
            else if (text.fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON"))
            {
                text.fontSharedMaterial.DisableKeyword("UNDERLAY_ON");
            }

            if (model.outlineWidth > 0f)
            {
                text.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
                text.outlineWidth = model.outlineWidth;
                text.outlineColor = model.outlineColor;
            }
            else if (text.fontSharedMaterial.IsKeywordEnabled("OUTLINE_ON"))
            {
                text.fontSharedMaterial.DisableKeyword("OUTLINE_ON");
            }
        }
    }
}