using DCL.Helpers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class TextShape : BaseComponent
    {
        public override string componentName => "TextShape";

        [System.Serializable]
        public class Model
        {
            [Header("Font Properties")]
            public string value = "";

            //Font properties
            public Color color = Color.white;
            public float opacity = 1f;
            public float fontSize = 10f;
            public string fontFamily = "Arial"; //NOTE(Brian): Should we make this a new object type?
            public string fontWeight = "normal";


            //Text box properties
            [Header("Text box properties")]
            public float hAlign = 0;
            public float vAlign = 0;
            public float width = 1f;
            public float height = 0.2f;
            public bool resizeToFit = false;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public float zIndex = 0f;
            public float lineSpacing = 0f;
            public int lineCount = 0;
            public bool textWrapping = false;

            //Text shadow + outline
            [Header("Text shadow properties")]
            public float shadowBlur = 0f;
            public float shadowOffsetX = 0f;
            public float shadowOffsetY = 0f;
            public Color shadowColor = new Color(1, 1, 1);
        }


        enum AlignFlags
        {
            CENTER = 0,

            LEFT = 1,
            TOP = 1 << 1,
            BOTTOM = 1 << 2,
            RIGHT = 1 << 3,

            TOP_LEFT = LEFT | TOP,
            TOP_RIGHT = RIGHT | TOP,
            BOTTOM_LEFT = LEFT | BOTTOM,
            BOTTOM_RIGHT = RIGHT | BOTTOM,
        }

        public Model model;
        public TextMeshPro text;
        public RectTransform rectTransform;

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);
            ApplyModelChanges(model);
            yield return null;
        }

        void ApplyModelChanges(Model model)
        {
            rectTransform.sizeDelta = new Vector2(model.width, model.height);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            text.text = model.value;

            text.color = new Color(model.color.r, model.color.g, model.color.b, model.opacity);
            text.fontSize = (int)model.fontSize / 20.0f;
            text.richText = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.enableAutoSizing = model.resizeToFit;
            text.margin =
                new Vector4
                (
                    (int)model.paddingLeft,
                    (int)model.paddingTop,
                    (int)model.paddingRight,
                    (int)model.paddingBottom
                );

            text.alignment = GetAlignment(model.hAlign, model.vAlign);
            text.lineSpacing = model.lineSpacing;
            text.maxVisibleLines = Mathf.Max(model.lineCount, 1);
            text.enableWordWrapping = model.textWrapping;

            if (model.shadowOffsetX == 0 && model.shadowOffsetY == 0)
            {
                if (text.fontMaterial.IsKeywordEnabled("UNDERLAY_ON"))
                {
                    text.fontMaterial.DisableKeyword("UNDERLAY_ON");
                }
            }
            else
            {
                text.fontMaterial.EnableKeyword("UNDERLAY_ON");
                text.fontMaterial.SetColor("_UnderlayColor", model.shadowColor);
                text.fontMaterial.SetFloat("_UnderlaySoftness", model.shadowBlur);
            }
        }

        public static TextAlignmentOptions GetAlignment(float hAlign, float vAlign)
        {
            int alignment = (int)AlignFlags.CENTER;

            if (hAlign <= 0)
            {
                alignment |= (int)AlignFlags.LEFT;
            }
            else if (hAlign >= 1)
            {
                alignment |= (int)AlignFlags.RIGHT;
            }

            if (vAlign <= 0)
            {
                alignment |= (int)AlignFlags.BOTTOM;
            }
            else if (vAlign >= 1)
            {
                alignment |= (int)AlignFlags.TOP;
            }


            switch ((AlignFlags)alignment)
            {
                case AlignFlags.LEFT:
                    return TextAlignmentOptions.Left;
                case AlignFlags.TOP:
                    return TextAlignmentOptions.Top;
                case AlignFlags.BOTTOM:
                    return TextAlignmentOptions.Bottom;
                case AlignFlags.RIGHT:
                    return TextAlignmentOptions.Right;
                case AlignFlags.CENTER:
                    return TextAlignmentOptions.Center;
                case AlignFlags.TOP_LEFT:
                    return TextAlignmentOptions.TopLeft;
                case AlignFlags.TOP_RIGHT:
                    return TextAlignmentOptions.TopRight;
                case AlignFlags.BOTTOM_LEFT:
                    return TextAlignmentOptions.BottomLeft;
                case AlignFlags.BOTTOM_RIGHT:
                    return TextAlignmentOptions.BottomRight;
                default:
                    return TextAlignmentOptions.Center;
            }
        }
    }
}
