using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapParcelHighlight : MonoBehaviour
{
    public enum HighlighStyle
    {
        DEFAULT = 0,
        BUILDER_ENABLE = 1,
        BUILDER_DISABLE = 2,
    }

    [Header("Builder in world style")]
    [SerializeField] internal Sprite builderHighlightTexture;
    [SerializeField] internal Sprite builderHighlightDisableTexture;

    [Header("Default style")]
    [SerializeField] internal Sprite defaultTexture;

    [Header("Normal map style")]
    [SerializeField] internal Image highlighImage;

    private Vector2 highlighSize;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetStyle(HighlighStyle style)
    {
        switch (style)
        {
            case HighlighStyle.DEFAULT:
                highlighImage.sprite = defaultTexture;
                ChangeHighlighSize(Vector2Int.one);
                break;
            case HighlighStyle.BUILDER_ENABLE:
                highlighImage.sprite = builderHighlightTexture;
                break;
            case HighlighStyle.BUILDER_DISABLE:
                highlighImage.sprite = builderHighlightDisableTexture;
                break;
        }
    }

    public void SetScale(float scale)
    {
        // This can happen if the set scale is set before the awake method
        if(rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        rectTransform.localScale = new Vector3(scale, scale, 1f);
        
        highlighImage.rectTransform.sizeDelta = Vector2.zero;
    }

    public void ChangeHighlighSize(Vector2Int newSize)
    {
        highlighSize = new Vector2(18 * newSize.x, 18 * newSize.y);
        rectTransform.sizeDelta = highlighSize;
    }
}