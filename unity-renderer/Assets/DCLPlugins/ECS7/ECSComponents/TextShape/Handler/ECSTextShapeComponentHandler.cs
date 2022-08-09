using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using TMPro;
using UnityEngine;

public class ECSTextShapeComponentHandler : IECSComponentHandler<PBTextShape>
{
    private static readonly int underlayColor = Shader.PropertyToID("_UnderlayColor");
    private static readonly int offsetX = Shader.PropertyToID("_UnderlayOffsetX");
    private static readonly int offsetY = Shader.PropertyToID("_UnderlayOffsetY");
    private static readonly int underlaySoftness = Shader.PropertyToID("_UnderlaySoftness");
    
    private const string BOTTOM = "bottom";
    private const string TOP = "top";
    private const string LEFT = "left";
    private const string RIGHT = "right";
    
    private const string COMPONENT_NAME = "TextShape";

    internal GameObject textGameObject;
    internal TextMeshPro textComponent;
    internal RectTransform rectTransform;
    internal AssetPromise_Font promise;
    
    private PBTextShape currentModel;
    private readonly DataStore_ECS7 dataStore;
    private readonly AssetPromiseKeeper_Font fontPromiseKeeper;
    
    private string lastFontUsed;

    public ECSTextShapeComponentHandler(DataStore_ECS7 dataStoreEcs7, AssetPromiseKeeper_Font fontPromiseKeeper)
    {
        dataStore = dataStoreEcs7;
        this.fontPromiseKeeper = fontPromiseKeeper;
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
    {
        textGameObject = new GameObject(COMPONENT_NAME);
        textGameObject.AddComponent<MeshRenderer>();
        rectTransform = textGameObject.AddComponent<RectTransform>();
        textComponent = textGameObject.AddComponent<TextMeshPro>();
        textGameObject.transform.SetParent(entity.gameObject.transform,false);
        dataStore.AddShapeReady(entity.entityId,textGameObject);
        textComponent.text = string.Empty;
        
        if (entity.meshRootGameObject == null)
            entity.meshesInfo.meshRootGameObject = textGameObject;
    }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        RemoveModelFromPending(scene);
        dataStore.RemoveShapeReady(entity.entityId);
        if (promise != null)
            fontPromiseKeeper.Forget(promise);
        GameObject.Destroy(textGameObject);

        textGameObject = null;
        textComponent = null;
        rectTransform = null;
        currentModel = null;
    }
    
    private PBTextShape NormalizeAndClone(PBTextShape model)
    {
        PBTextShape normalizedModel = model.Clone();
            
        normalizedModel.Visible = !model.HasVisible || model.Visible;
        normalizedModel.Opacity = model.HasOpacity ? model.Opacity : 1.0f;
        normalizedModel.HTextAlign = model.HasHTextAlign ? model.HTextAlign : "center";
        normalizedModel.VTextAlign = model.HasVTextAlign ? model.VTextAlign : "center";
        normalizedModel.Width = model.HasWidth ? model.Width : 1.0f;
        normalizedModel.Height = model.HasHeight ? model.Height : 1.0f;
        normalizedModel.ShadowColor = model.ShadowColor != null ? model.ShadowColor : new Color3() { R = 1.0f, G = 1.0f, B = 1.0f };
        normalizedModel.OutlineColor = model.OutlineColor != null ? model.OutlineColor : new Color3() { R = 1.0f, G = 1.0f, B = 1.0f };
        normalizedModel.TextColor = model.TextColor != null ? model.TextColor : new Color3() { R = 1.0f, G = 1.0f, B = 1.0f };

        return normalizedModel;
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTextShape model)
    {
        if (model.Equals(currentModel))
            return;
        
        currentModel = model;

        PrepareRectTransform(model);
        
        // If we use the same font than the last time, we just update the model, if not, we download it and apply the changes after the download
        if (lastFontUsed != null && lastFontUsed == model.Font)
        {
            ApplyModelChanges(entity, model);
        }
        else
        {
            lastFontUsed = model.Font;
            dataStore.AddPendingResource(scene.sceneData.id, model);
            promise = new AssetPromise_Font(model.Font);
            promise.OnSuccessEvent += assetFont =>
            {
                textComponent.font = assetFont.font;
                ApplyModelChanges(entity, model);

                RemoveModelFromPending(scene);
            };
            promise.OnFailEvent += ( mesh,  exception) =>
            {
                RemoveModelFromPending(scene);
            };

            fontPromiseKeeper.Keep(promise);
        }
    }

    private void RemoveModelFromPending(IParcelScene scene)
    {
        if (currentModel != null)
            dataStore.RemovePendingResource(scene.sceneData.id, currentModel);

        currentModel = null;
    }

    private void PrepareRectTransform(PBTextShape model)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        
        // NOTE: previously width and height weren't working (setting sizeDelta before anchors and offset result in
        // sizeDelta being reset to 0,0)
        // to fix textWrapping and avoid backwards compatibility issues as result of the size being properly set (like text alignment)
        // we only set it if textWrapping is enabled.
        if (model.TextWrapping)
        {
            rectTransform.sizeDelta = new Vector2(model.Width, model.Height);
        }
        else
        {
            rectTransform.sizeDelta = Vector2.zero;
        }
    }
    
    internal void ApplyModelChanges(IDCLEntity entity, PBTextShape model)
    {
        textComponent.text = model.Text;

        if (model.TextColor != null)
            textComponent.color = new UnityEngine.Color(model.TextColor.R, model.TextColor.G, model.TextColor.B, model.Opacity);

        textComponent.fontSize = model.FontSize;
        textComponent.richText = true;
        textComponent.overflowMode = TextOverflowModes.Overflow;
        textComponent.enableAutoSizing = model.FontAutoSize;

        textComponent.margin =
            new Vector4
            (
                model.PaddingLeft,
                model.PaddingTop,
                model.PaddingRight,
                model.PaddingBottom
            );

        textComponent.alignment = GetAlignment(model.VTextAlign, model.HTextAlign);
        textComponent.lineSpacing = model.LineSpacing;

        if (model.LineCount != 0)
        {
            textComponent.maxVisibleLines = Mathf.Max(model.LineCount, 1);
        }
        else
        {
            textComponent.maxVisibleLines = int.MaxValue;
        }

        textComponent.enableWordWrapping = model.TextWrapping && !textComponent.enableAutoSizing;
        
        // Shadows
        bool underlayKeywordEnabled = false;
        if (!Mathf.Approximately(model.ShadowBlur,0))
        {
            textComponent.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
            textComponent.fontSharedMaterial.SetFloat(underlaySoftness, model.ShadowBlur);
            underlayKeywordEnabled = true;
        }
        
        if (model.ShadowColor != null)
        {
            if (!underlayKeywordEnabled)
            {
                textComponent.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
                underlayKeywordEnabled = true;
            }
            var shadowColor  = new UnityEngine.Color(model.ShadowColor.R, model.ShadowColor.G, model.ShadowColor.B, model.Opacity);
            textComponent.fontSharedMaterial.SetColor(underlayColor, shadowColor);
            textComponent.fontSharedMaterial.SetFloat(offsetX, model.ShadowOffsetX);
            textComponent.fontSharedMaterial.SetFloat(offsetY, model.ShadowOffsetY);
        }
        
        if (!underlayKeywordEnabled && textComponent.fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON"))
        {
            textComponent.fontSharedMaterial.DisableKeyword("UNDERLAY_ON");
        }

        // Outline
        if (model.OutlineWidth > 0f)
        {
            textComponent.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
            textComponent.outlineWidth = model.OutlineWidth;
            if (model.OutlineColor != null)
            {
                var outlineColor  = new UnityEngine.Color(model.OutlineColor.R, model.OutlineColor.G, model.OutlineColor.B, 1);
                textComponent.outlineColor = outlineColor;
            }
        }
        else if (textComponent.fontSharedMaterial.IsKeywordEnabled("OUTLINE_ON"))
        {
            textComponent.fontSharedMaterial.DisableKeyword("OUTLINE_ON");
        }

        textGameObject.SetActive(model.Visible);
        entity.OnShapeUpdated?.Invoke(entity);
    }

    internal TextAlignmentOptions GetAlignment(string vTextAlign, string hTextAlign)
    {
        vTextAlign = vTextAlign.ToLower();
        hTextAlign = hTextAlign.ToLower();

        switch (vTextAlign)
        {
            case TOP:
                switch (hTextAlign)
                {
                    case LEFT:
                        return TextAlignmentOptions.TopLeft;
                    case RIGHT:
                        return TextAlignmentOptions.TopRight;
                    default:
                        return TextAlignmentOptions.Top;
                }

            case BOTTOM:
                switch (hTextAlign)
                {
                    case LEFT:
                        return TextAlignmentOptions.BottomLeft;
                    case RIGHT:
                        return TextAlignmentOptions.BottomRight;
                    default:
                        return TextAlignmentOptions.Bottom;
                }

            default: // center
                switch (hTextAlign)
                {
                    case LEFT:
                        return TextAlignmentOptions.Left;
                    case RIGHT:
                        return TextAlignmentOptions.Right;
                    default:
                        return TextAlignmentOptions.Center;
                }
        }
    }
}
