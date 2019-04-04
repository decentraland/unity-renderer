using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIInputText : UIShape
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public TextShape.Model textModel;
            public string placeholder;
            public Color placeholderColor;
            public Color focusedBackground;
            public string onTextSubmitEvent;
        }

        public new Model model
        {
            get { return base.model as Model; }
            set { base.model = value; }
        }

        public TMP_Text tmpText => refContainer.text;
        public TMP_InputField inputField => refContainer.inputField;
        public RectTransform rectTransform => refContainer.rectTransform;

        public InputTextRefContainer refContainer;

        public UIInputText(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): We have to serialize twice now, but in the future we should fix the
            //             client data structure to be like this, so we can serialize all of it in one shot.
            model = JsonUtility.FromJson<Model>(newJson);

            if (!scene.isTestScene)
                model.textModel = JsonUtility.FromJson<TextShape.Model>(newJson);

            if (refContainer == null)
            {
                refContainer = InstantiateUIGameObject<InputTextRefContainer>("InputText");
                // Configure transform reference used by future children ui components
                childHookRectTransform = refContainer.inputField.GetComponent<RectTransform>();
            }
            else
            {
                ReparentComponent(refContainer.rectTransform, model.parentComponent);
            }

            inputField.enabled = model.visible;
            inputField.textComponent.enabled = model.visible;
            inputField.textViewport = scene.uiScreenSpaceCanvas.transform as RectTransform;

            inputField.onSelect.AddListener(OnFocus);
            inputField.onDeselect.AddListener(OnBlur);
            inputField.onSubmit.AddListener(OnSubmit);

            DCL.Components.TextShape.ApplyModelChanges(tmpText, model.textModel);

            RectTransform parentRT = refContainer.rectTransform;

            yield return ResizeAlignAndReposition(  targetTransform:        refContainer.textLayoutElementRT,
                                                    parentWidth:            parentRT.rect.width,
                                                    parentHeight:           parentRT.rect.height,
                                                    alignmentLayout:        refContainer.alignmentLayoutGroup,
                                                    alignedLayoutElement:   refContainer.textLayoutElement);

            inputField.text = model.placeholder;
            inputField.textComponent.color = new Color( model.placeholderColor.r, model.placeholderColor.g, model.placeholderColor.b, model.textModel.opacity );
            refContainer.bgImage.color = new Color( model.focusedBackground.r, model.focusedBackground.g, model.focusedBackground.b, model.textModel.opacity );
            yield break;
        }

        public void OnFocus(string call)
        {
            if (inputField.text == model.placeholder)
                inputField.text = "";

            inputField.customCaretColor = true;
            inputField.caretColor = Color.white;
        }

        public void OnBlur(string call)
        {
            HideCaret();
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
            Interface.WebInterface.ReportOnTextSubmitEvent(scene.sceneData.id, model.onTextSubmitEvent, tmpText.text);

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
