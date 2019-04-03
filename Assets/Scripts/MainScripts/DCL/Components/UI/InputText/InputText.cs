using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components.UI
{
    public class InputText : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public DCL.Components.TextShape.Model textModel;
            public string placeholder;
            public Color placeholderColor;
            public Color focusedBackground;
            public string onTextSubmitEvent;
        }

        new Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        public TMP_Text text => refContainer.text;
        public TMP_InputField inputField => refContainer.inputField;
        public RectTransform rectTransform => refContainer.rectTransform;

        public InputTextRefContainer refContainer;

        public InputText(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): We have to serialize twice now, but in the future we should fix the
            //             client data structure to be like this, so we can serialize all of it in one shot.
            model = JsonUtility.FromJson<Model>(newJson);
            model.textModel = JsonUtility.FromJson<DCL.Components.TextShape.Model>(newJson);

            if (refContainer == null)
            {
                refContainer = InstantiateUIGameObject<InputTextRefContainer>("InputText");
            }

            inputField.enabled = model.visible;
            inputField.textComponent.enabled = model.visible;
            inputField.textViewport = scene.uiScreenSpaceCanvas.transform as RectTransform;

            inputField.onSelect.AddListener(OnFocus);
            inputField.onDeselect.AddListener(OnBlur);
            inputField.onSubmit.AddListener(OnSubmit);

            DCL.Components.TextShape.ApplyModelChanges(text, model.textModel);
            RectTransform prefabRT = refContainer.GetComponent<RectTransform>();

            prefabRT.sizeDelta = Vector2.zero;

            prefabRT.anchorMin = Vector2.zero;
            prefabRT.anchorMax = Vector2.one;
            prefabRT.offsetMin = Vector2.zero;
            prefabRT.offsetMax = Vector2.one;
            prefabRT.ForceUpdateRectTransforms();

            RectTransform parentRecTransform = refContainer.GetComponentInParent<RectTransform>();

            RectTransform targetTransform = refContainer.rectTransform;
            float parentWidth = parentRecTransform.rect.width;
            float parentHeight = parentRecTransform.rect.height;
            var alignedLayoutElement = refContainer.textLayoutElement;
            var alignmentLayout = refContainer.alignmentLayoutGroup;

            RectTransform layoutElementRT = alignedLayoutElement.GetComponent<RectTransform>();
            // Resize
            layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, model.width.GetScaledValue(parentWidth));
            layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, model.height.GetScaledValue(parentHeight));
            layoutElementRT.ForceUpdateRectTransforms();

            // Alignment (Alignment uses size so we should always align AFTER reisizing)
            alignedLayoutElement.ignoreLayout = false;
            ConfigureAlignment(alignmentLayout);
            LayoutRebuilder.ForceRebuildLayoutImmediate(alignmentLayout.transform as RectTransform);
            alignedLayoutElement.ignoreLayout = true;

            // Reposition
            targetTransform.localPosition += new Vector3(model.positionX.GetScaledValue(parentWidth),
                                                            model.positionY.GetScaledValue(parentHeight), 0f);

            inputField.text = model.placeholder;
            inputField.textComponent.color = new Color(model.placeholderColor.r, model.placeholderColor.g, model.placeholderColor.b, model.textModel.opacity);
            refContainer.bgImage.color = new Color(model.focusedBackground.r, model.focusedBackground.g, model.focusedBackground.b, model.textModel.opacity);
            yield break;
        }

        private void OnFocus(string call)
        {
            if (inputField.text == model.placeholder)
                inputField.text = "";

            inputField.customCaretColor = true;
            inputField.caretColor = Color.white;
        }

        private void OnBlur(string call)
        {
            HideCaret();
        }

        void HideCaret()
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

        private void OnSubmit(string call)
        {
            Interface.WebInterface.ReportOnTextSubmitEvent(scene.sceneData.id, model.onTextSubmitEvent, text.text);

            inputField.text = "";
            inputField.caretColor = Color.white;
            inputField.Select();
            inputField.ActivateInputField();
        }

        public override void Dispose()
        {
            inputField.onSelect.RemoveAllListeners();
            inputField.onDeselect.RemoveAllListeners();
            inputField.onSubmit.RemoveAllListeners();
            base.Dispose();
        }

    }
}
