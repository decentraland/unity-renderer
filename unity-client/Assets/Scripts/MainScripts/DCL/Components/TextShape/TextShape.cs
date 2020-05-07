using System.Collections;
using DCL.Controllers;
using TMPro;
using UnityEngine;

namespace DCL.Components
{
    public class TextShape : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public bool billboard;

            [Header("Font Properties")]
            public string value = "";

            public Color color = Color.white;
            public float opacity = 1f;
            public float fontSize = 100f;
            public bool fontAutoSize = false;
            public string fontWeight = "normal";
            public string font;

            [Header("Text box properties")]
            public string hTextAlign = "bottom";

            public string vTextAlign = "left";
            public float width = 1f;
            public float height = 0.2f;
            public bool adaptWidth = false;
            public bool adaptHeight = false;
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
        }

        public Model model;
        public TextMeshPro text;
        public RectTransform rectTransform;

        public void Update()
        {
            if (model.billboard && Camera.main != null)
            {
                transform.forward = Camera.main.transform.forward;
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            rectTransform.sizeDelta = new Vector2(model.width, model.height);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            yield return ApplyModelChanges(scene, text, model);
        }

        public static IEnumerator ApplyModelChanges(ParcelScene scene, TMP_Text text, Model model)
        {
            if (!string.IsNullOrEmpty(model.font))
            {
                yield return DCLFont.SetFontFromComponent(scene, model.font, text);
            }

            text.text = model.value;

            text.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);
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
                text.fontMaterial.EnableKeyword("UNDERLAY_ON");
                text.fontMaterial.SetColor("_UnderlayColor", model.shadowColor);
                text.fontMaterial.SetFloat("_UnderlaySoftness", model.shadowBlur);
            }
            else if (text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"))
            {
                text.fontMaterial.DisableKeyword("UNDERLAY_ON");
            }

            if (model.outlineWidth > 0f)
            {
                text.fontMaterial.EnableKeyword("OUTLINE_ON");
                text.outlineWidth = model.outlineWidth;
                text.outlineColor = model.outlineColor;
            }
            else if (text.fontMaterial.IsKeywordEnabled("OUTLINE_ON"))
            {
                text.fontMaterial.DisableKeyword("OUTLINE_ON");
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
    }
}