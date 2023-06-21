using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using Decentraland.Common;
using TMPro;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class ECSTextShapeComponentHandler : IECSComponentHandler<PBTextShape>
{
    private static readonly int UNDERLAY_COLOR_SHADER_PROP = Shader.PropertyToID("_UnderlayColor");
    private static readonly int UNDERLAY_SOFTNESS_SHADER_PROP = Shader.PropertyToID("_UnderlaySoftness");

    private readonly AssetPromiseKeeper_Font fontPromiseKeeper;
    private readonly IInternalECSComponent<InternalRenderers> renderersInternalComponent;
    private readonly IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent;

    internal TextMeshPro textComponent;
    internal AssetPromise_Font fontPromise;

    private IParcelScene scene;
    private IDCLEntity entity;
    private GameObject textGameObject;
    private RectTransform rectTransform;
    private Renderer textRenderer;
    private PBTextShape currentModel;

    public ECSTextShapeComponentHandler(AssetPromiseKeeper_Font fontPromiseKeeper, IInternalECSComponent<InternalRenderers> renderersInternalComponent, IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent)
    {
        this.fontPromiseKeeper = fontPromiseKeeper;
        this.renderersInternalComponent = renderersInternalComponent;
        this.sbcInternalComponent = sbcInternalComponent;
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
    {
        this.scene = scene;
        this.entity = entity;

        textGameObject = new GameObject("TextShape");

        rectTransform = textGameObject.AddComponent<RectTransform>();
        textComponent = textGameObject.AddComponent<TextMeshPro>();
        textRenderer = textComponent.renderer;
        rectTransform.SetParent(entity.gameObject.transform, false);

        textComponent.text = string.Empty;
        textComponent.richText = true;
        textComponent.overflowMode = TextOverflowModes.Overflow;
        textComponent.OnPreRenderText += OnTextRendererUpdated;
        renderersInternalComponent.AddRenderer(scene, entity, textRenderer);
    }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        textComponent.OnPreRenderText -= OnTextRendererUpdated;
        renderersInternalComponent.RemoveRenderer(scene, entity, textRenderer);

        fontPromiseKeeper.Forget(fontPromise);

        Object.Destroy(textGameObject);
    }

    private void OnTextRendererUpdated(TMP_TextInfo tmproInfo)
    {
        var model = sbcInternalComponent.GetFor(scene, entity)?.model;
        if (model == null) return;

        InternalSceneBoundsCheck finalModel = model.Value;
        finalModel.meshesDirty = true;

        sbcInternalComponent.PutFor(scene, entity, finalModel);
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTextShape model)
    {
        if (model.Equals(currentModel))
            return;

        currentModel = model;

        SetRectTransform(rectTransform, model.GetTextWrapping() ? new Vector2(model.GetWidth(), model.GetHeight()) : Vector2.zero);

        textComponent.text = model.Text;

        var color = model.GetTextColor().ToUnityColor();
        textComponent.color = color;

        textComponent.fontSize = model.GetFontSize();
        textComponent.enableAutoSizing = model.GetFontAutoSize();
        textComponent.margin = model.GetPadding();
        textComponent.alignment = GetAlignment(model.GetTextAlign());
        textComponent.lineSpacing = model.GetLineSpacing();
        textComponent.maxVisibleLines = model.GetLineCount() != 0 ? Mathf.Max(model.GetLineCount(), 1) : int.MaxValue;
        textComponent.textWrappingMode = model.GetTextWrapping() && !model.GetFontAutoSize() ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;

        var prevPromise = fontPromise;
        fontPromise = new AssetPromise_Font(model.GetFont().ToFontName());

        fontPromise.OnSuccessEvent += assetFont =>
        {
            textComponent.font = assetFont.font;
            SetShadow(textComponent.fontSharedMaterial, model.GetShadowOffsetX(), model.GetShadowOffsetY(), model.GetShadowBlur(), model.GetShadowColor());
            SetOutline(textComponent, model.GetOutlineWidth(), model.GetOutlineColor());
        };

        fontPromiseKeeper.Keep(fontPromise);
        fontPromiseKeeper.Forget(prevPromise);
    }

    private static void SetRectTransform(RectTransform rectTransform, Vector2 sizeDelta)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.sizeDelta = sizeDelta;
    }

    private static void SetShadow(Material fontMaterial, float shadowOffsetX, float shadowOffsetY,
        float shadowBlur, Color3 shadowColor)
    {
        const string UNDERLAY_ON_KERYWORD = "UNDERLAY_ON";

        if (shadowOffsetX != 0 || shadowOffsetY != 0)
        {
            fontMaterial.EnableKeyword(UNDERLAY_ON_KERYWORD);
            fontMaterial.SetColor(UNDERLAY_COLOR_SHADER_PROP, shadowColor.ToUnityColor());
            fontMaterial.SetFloat(UNDERLAY_SOFTNESS_SHADER_PROP, shadowBlur);
        }
        else if (fontMaterial.IsKeywordEnabled(UNDERLAY_ON_KERYWORD))
        {
            fontMaterial.DisableKeyword(UNDERLAY_ON_KERYWORD);
        }
    }

    private static void SetOutline(TextMeshPro textComponent, float outlineWidth, Color3 outlineColor)
    {
        const string OUTLINE_ON_KERYWORD = "OUTLINE_ON";

        Material fontSharedMaterial = textComponent.fontSharedMaterial;

        if (outlineWidth > 0f)
        {
            fontSharedMaterial.EnableKeyword(OUTLINE_ON_KERYWORD);
            textComponent.outlineWidth = outlineWidth;
            textComponent.outlineColor = outlineColor.ToUnityColor();
        }
        else if (fontSharedMaterial.IsKeywordEnabled(OUTLINE_ON_KERYWORD))
        {
            fontSharedMaterial.DisableKeyword(OUTLINE_ON_KERYWORD);
        }
    }

    private static TextAlignmentOptions GetAlignment(TextAlignMode value)
    {
        switch (value)
        {
            case TextAlignMode.TamTopLeft:
                return TextAlignmentOptions.TopLeft;
            case TextAlignMode.TamTopRight:
                return TextAlignmentOptions.TopRight;
            case TextAlignMode.TamTopCenter:
                return TextAlignmentOptions.Top;
            case TextAlignMode.TamBottomLeft:
                return TextAlignmentOptions.BottomLeft;
            case TextAlignMode.TamBottomRight:
                return TextAlignmentOptions.BottomRight;
            case TextAlignMode.TamBottomCenter:
                return TextAlignmentOptions.Bottom;
            case TextAlignMode.TamMiddleLeft:
                return TextAlignmentOptions.Left;
            case TextAlignMode.TamMiddleCenter:
                return TextAlignmentOptions.Center;
            case TextAlignMode.TamMiddleRight:
                return TextAlignmentOptions.Right;
            default:
                return TextAlignmentOptions.Center;
        }
    }
}
