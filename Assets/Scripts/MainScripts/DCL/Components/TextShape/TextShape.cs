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
            public string value = "Hello world!";

            //Font properties
            public Color color = Color.white;
            public float opacity = 1;
            public float fontSize = 10;
            public string fontFamily; //NOTE(Brian): Should we make this a new object type?
            public string fontWeight;


            //Text box properties
            [Header("Text box properties")]
            public float hAlign;
            public float vAlign;
            public float width = 10;
            public float height = 10;
            public bool resizeToFit;
            public float paddingTop;
            public float paddingRight;
            public float paddingBottom;
            public float paddingLeft;
            public float zIndex;
            public float lineSpacing;
            public int lineCount;
            public bool textWrapping;

            //Text shadow + outline
            [Header("Text shadow properties")]
            public float shadowBlur;
            public float shadowOffsetX;
            public float shadowOffsetY;
            public Color shadowColor;
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

        public TextMeshProUGUI text;
        public Canvas canvas;

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);
            ApplyModelChanges(model);
            yield return null;
        }

        void ApplyModelChanges(Model model)
        {
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            canvasRectTransform.sizeDelta = new Vector2(model.width, model.height);

            RectTransform textRectTransform = text.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;

            text.text = model.value;

            text.color = new Color(model.color.r, model.color.g, model.color.b, model.opacity);
            text.fontSize = (int)model.fontSize;
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
            text.maxVisibleLines = model.lineCount;
            text.enableWordWrapping = model.textWrapping;

            text.Rebuild(CanvasUpdate.PreRender);

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
