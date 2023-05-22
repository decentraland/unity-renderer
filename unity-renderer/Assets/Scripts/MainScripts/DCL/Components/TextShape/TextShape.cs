using System;
using System.Collections;
using DCL.Helpers;
using DCL.Models;
using TMPro;
using UnityEngine;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

namespace DCL.Components
{
    public class TextShape : BaseComponent
    {
        [Serializable]
        public class Model : BaseModel
        {
            public bool billboard;

            [Header("Font Properties")]
            public string value = "";

            public bool visible = true;

            public Color color = Color.white;
            public float opacity = 1f;
            public float fontSize = 100f;
            public bool fontAutoSize = false;
            public string font;

            [Header("Text box properties")]
            public string hTextAlign = "bottom";
            public string vTextAlign = "left";
            public float width = 1f;
            public float height = 0.2f;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public float lineSpacing = 0f;
            public int lineCount = 0;
            public bool textWrapping = false;

            [Header("Text shadow properties")]
            public float shadowBlur = 0f;
            public float shadowOffsetX = 0f;
            public float shadowOffsetY = 0f;
            public Color shadowColor = new Color(1, 1, 1);

            [Header("Text outline properties")]
            public float outlineWidth = 0f;

            public Color outlineColor = Color.white;

            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.TextShape)
                    return Utils.SafeUnimplemented<TextShape, Model>(expected: ComponentBodyPayload.PayloadOneofCase.TextShape, actual: pbModel.PayloadCase);

                var model = new Model();

                try {
                    if (pbModel.TextShape.HasBillboard) model.billboard = pbModel.TextShape.Billboard;
                    if (pbModel.TextShape.Color != null) model.color = pbModel.TextShape.Color.AsUnityColor();
                    if (pbModel.TextShape.HasFont) model.font = pbModel.TextShape.Font;
                    if (pbModel.TextShape.HasHeight) model.height = pbModel.TextShape.Height;
                    if (pbModel.TextShape.HasOpacity) model.opacity = pbModel.TextShape.Opacity;
                    if (pbModel.TextShape.HasValue) model.value = pbModel.TextShape.Value;
                    if (pbModel.TextShape.HasVisible) model.visible = pbModel.TextShape.Visible;
                    if (pbModel.TextShape.HasWidth) model.width = pbModel.TextShape.Width;
                    if (pbModel.TextShape.HasLineCount) model.lineCount = pbModel.TextShape.LineCount;
                    if (pbModel.TextShape.OutlineColor != null) model.outlineColor = pbModel.TextShape.OutlineColor.AsUnityColor();
                    if (pbModel.TextShape.HasOutlineWidth) model.outlineWidth = pbModel.TextShape.OutlineWidth;
                    if (pbModel.TextShape.HasPaddingBottom ) model.paddingBottom = pbModel.TextShape.PaddingBottom;
                    if (pbModel.TextShape.HasPaddingLeft) model.paddingLeft = pbModel.TextShape.PaddingLeft;
                    if (pbModel.TextShape.HasPaddingRight) model.paddingRight = pbModel.TextShape.PaddingRight;
                    if (pbModel.TextShape.HasPaddingTop) model.paddingTop = pbModel.TextShape.PaddingTop;
                    if (pbModel.TextShape.HasShadowBlur) model.shadowBlur = pbModel.TextShape.ShadowBlur;
                    if (pbModel.TextShape.ShadowColor != null) model.shadowColor = pbModel.TextShape.ShadowColor.AsUnityColor();
                    if (pbModel.TextShape.HasShadowOffsetX) model.shadowOffsetX = pbModel.TextShape.ShadowOffsetX;
                    if (pbModel.TextShape.HasShadowOffsetY) model.shadowOffsetY = pbModel.TextShape.ShadowOffsetY;
                    if (pbModel.TextShape.HasTextWrapping) model.textWrapping = pbModel.TextShape.TextWrapping;
                    if (pbModel.TextShape.HasVTextAlign) model.vTextAlign = pbModel.TextShape.VTextAlign;
                    if (pbModel.TextShape.HasHTextAlign) model.hTextAlign = pbModel.TextShape.HTextAlign;
                    if (pbModel.TextShape.HasFontSize) model.fontAutoSize = pbModel.TextShape.FontSize == 0;
                    if (pbModel.TextShape.HasFontSize) model.fontSize = pbModel.TextShape.FontSize;
                    // if (pbModel.TextShape.HasLineSpacing) model.lineSpacing = float.Parse(pbModel.TextShape.LineSpacing[..^2]); // TODO [SDK6_REFACTOR] revise this, is failing
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return model;
            }

        }

        public MeshRenderer meshRenderer;
        public TextMeshPro text;
        public RectTransform rectTransform;
        private Model cachedModel;
        private Material cachedFontMaterial;
        private Camera mainCamera;
        private bool cameraFound;

        public override string componentName => "text";


        private bool CameraFound
        {
            get
            {
                if (!cameraFound)
                {
                    mainCamera = Camera.main;
                    if (mainCamera != null)
                        cameraFound = true;
                }

                return cameraFound;
            }
        }


        private void Awake()
        {
            model = new Model();

            cachedFontMaterial = new Material(text.fontSharedMaterial);
            text.fontSharedMaterial = cachedFontMaterial;
            text.text = string.Empty;

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, OnUpdate);
        }

        private void OnUpdate()
        {
            // Cameras are not detected while loading, so we can not load the camera on Awake or Start
            if (cachedModel.billboard && CameraFound)
                transform.forward = mainCamera.transform.forward;
        }


        new public Model GetModel() { return cachedModel; }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (rectTransform == null)
                yield break;

            Model model = (Model)newModel;
            cachedModel = model;
            PrepareRectTransform();

            // We avoid using even yield break; as this instruction skips a frame and we don't want that.
            if (!DCLFont.IsFontLoaded(scene, model.font))
            {
                yield return DCLFont.WaitUntilFontIsReady(scene, model.font);
            }

            DCLFont.SetFontFromComponent(scene, model.font, text);

            ApplyModelChanges(text, model);

            if (entity.meshRootGameObject == null)
                entity.meshesInfo.meshRootGameObject = gameObject;

            entity.OnShapeUpdated?.Invoke(entity);
        }

        public static void ApplyModelChanges(TMP_Text text, Model model)
        {
            text.text = model.value;

            text.color = new Color(model.color.r, model.color.g, model.color.b, model.visible ? model.opacity : 0);
            text.fontSize = (int)model.fontSize;
            text.richText = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.enableAutoSizing = model.fontAutoSize;

            text.margin =
                new Vector4
                (
                    (int)model.paddingLeft,
                    (int)model.paddingTop,
                    (int)model.paddingRight,
                    (int)model.paddingBottom
                );

            text.alignment = GetAlignment(model.vTextAlign, model.hTextAlign);
            text.lineSpacing = model.lineSpacing;

            if (model.lineCount != 0)
            {
                text.maxVisibleLines = Mathf.Max(model.lineCount, 1);
            }
            else
            {
                text.maxVisibleLines = int.MaxValue;
            }

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

        public static TextAlignmentOptions GetAlignment(string vTextAlign, string hTextAlign)
        {
            vTextAlign = vTextAlign.ToLower();
            hTextAlign = hTextAlign.ToLower();

            switch (vTextAlign)
            {
                case "top":
                    switch (hTextAlign)
                    {
                        case "left":
                            return TextAlignmentOptions.TopLeft;
                        case "right":
                            return TextAlignmentOptions.TopRight;
                        default:
                            return TextAlignmentOptions.Top;
                    }

                case "bottom":
                    switch (hTextAlign)
                    {
                        case "left":
                            return TextAlignmentOptions.BottomLeft;
                        case "right":
                            return TextAlignmentOptions.BottomRight;
                        default:
                            return TextAlignmentOptions.Bottom;
                    }

                default: // center
                    switch (hTextAlign)
                    {
                        case "left":
                            return TextAlignmentOptions.Left;
                        case "right":
                            return TextAlignmentOptions.Right;
                        default:
                            return TextAlignmentOptions.Center;
                    }
            }
        }

        private void PrepareRectTransform()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // NOTE: previously width and height weren't working (setting sizeDelta before anchors and offset result in
            // sizeDelta being reset to 0,0)
            // to fix textWrapping and avoid backwards compatibility issues as result of the size being properly set (like text alignment)
            // we only set it if textWrapping is enabled.
            if (cachedModel.textWrapping)
            {
                rectTransform.sizeDelta = new Vector2(cachedModel.width, cachedModel.height);
            }
            else
            {
                rectTransform.sizeDelta = Vector2.zero;
            }
        }

        public override int GetClassId() { return (int)CLASS_ID_COMPONENT.TEXT_SHAPE; }

        public override void Cleanup()
        {
            text.text = string.Empty;
            base.Cleanup();
        }

        private void OnDestroy()
        {
            Environment.i.platform.updateEventHandler?.RemoveListener(IUpdateEventHandler.EventType.Update, OnUpdate);

            base.Cleanup();
            Destroy(cachedFontMaterial);
        }
    }
}
