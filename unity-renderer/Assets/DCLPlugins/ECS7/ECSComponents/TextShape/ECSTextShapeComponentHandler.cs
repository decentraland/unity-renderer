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
    private GameObject textGameObject;
    private TextMeshPro textComponent;
    private RectTransform rectTransform;
    private PBTextShape model;
    
    internal AssetPromise_Font promise;
    private readonly DataStore_ECS7 dataStore;
    
    public ECSTextShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
    {
        dataStore = dataStoreEcs7;
    }
    
    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
    {
        textGameObject = new GameObject("TextShape");
        textGameObject.AddComponent<MeshRenderer>();
        rectTransform = textGameObject.AddComponent<RectTransform>();
        textComponent = textGameObject.AddComponent<TextMeshPro>();
        textGameObject.transform.SetParent(scene.GetSceneTransform());
    }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        RemoveModelFromPending(scene);
        if (promise != null)
            AssetPromiseKeeper_Font.i.Forget(promise);
        GameObject.Destroy(textGameObject);
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBTextShape model)
    {
        this.model = model;
        
        PrepareRectTransform();
        dataStore.AddPendingResource(scene.sceneData.id, model);
        promise = new AssetPromise_Font(model.Font);
        promise.OnSuccessEvent += assetFont =>
        {
            textComponent.font = assetFont.font;
            ApplyModelChanges(textComponent, model);
            RemoveModelFromPending(scene);
        };
        promise.OnFailEvent += ( mesh,  exception) =>
        {
            RemoveModelFromPending(scene);
        };
    }

    private void RemoveModelFromPending(IParcelScene scene)
    {
        if(model != null)
            dataStore.RemovePendingResource(scene.sceneData.id, model);

        model = null;
    }
    
    private void PrepareRectTransform()
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }
       public void ApplyModelChanges(TMP_Text text, PBTextShape model)
        {
            text.text = model.Text;

            text.color = new UnityEngine.Color(model.TextColor.Red,model.TextColor.Green, model.TextColor.Blue,1);
            text.fontSize = model.FontSize;
            text.richText = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.enableAutoSizing = model.FontAutoSize;

            text.margin =
                new Vector4
                (
                    model.PaddingLeft,
                    model.PaddingTop,
                    model.PaddingRight,
                    model.PaddingBottom
                );

            text.alignment = GetAlignment(model.VTextAlign, model.HTextAlign);
            text.lineSpacing = model.LineSpacing;

            if (model.LineCount != 0)
            {
                text.maxVisibleLines = Mathf.Max(model.LineCount, 1);
            }
            else
            {
                text.maxVisibleLines = int.MaxValue;
            }

            text.enableWordWrapping = model.TextWrapping && !text.enableAutoSizing;

            if (model.ShadowOffsetX != 0 || model.ShadowOffsetY != 0)
            {
                var shadowColor  = new UnityEngine.Color(model.ShadowColor.Red,model.ShadowColor.Green, model.ShadowColor.Blue,1);
                text.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
                text.fontSharedMaterial.SetColor("_UnderlayColor", shadowColor);
                text.fontSharedMaterial.SetFloat("_UnderlaySoftness", model.ShadowBlur);
            }
            else if (text.fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON"))
            {
                text.fontSharedMaterial.DisableKeyword("UNDERLAY_ON");
            }

            if (model.OutlineWidth > 0f)
            {
                var outlineColor  = new UnityEngine.Color(model.OutlineColor.Red,model.OutlineColor.Green, model.OutlineColor.Blue,1);
                text.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
                text.outlineWidth = model.OutlineWidth;
                text.outlineColor = outlineColor;
            }
            else if (text.fontSharedMaterial.IsKeywordEnabled("OUTLINE_ON"))
            {
                text.fontSharedMaterial.DisableKeyword("OUTLINE_ON");
            }
            
            textGameObject.SetActive(model.Visible);
        }

        public TextAlignmentOptions GetAlignment(string vTextAlign, string hTextAlign)
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
