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
    [SerializeField] internal Texture builderHighlightTexture;
    [SerializeField] internal Texture builderHighlightDisableTexture;

    [Header("Default style")]
    [SerializeField] internal Texture defaultTexture;

    [Header("Normal map style")]
    [SerializeField] internal RawImage highlighImage;

    private Vector2 highlighSize;

    public void SetStyle(HighlighStyle style)
    {
        switch (style)
        {
            case HighlighStyle.DEFAULT:
                highlighImage.texture = defaultTexture;
                ChangeHighlighSize(Vector2Int.one);
                break;
            case HighlighStyle.BUILDER_ENABLE:
                highlighImage.texture = builderHighlightTexture;
                break;
            case HighlighStyle.BUILDER_DISABLE:
                highlighImage.texture = builderHighlightDisableTexture;
                break;
        }
    }

    public void ChangeHighlighSize(Vector2Int newSize)
    {
        highlighSize = new Vector2(18 * newSize.x, 18 * newSize.y);
        highlighImage.rectTransform.sizeDelta = highlighSize;
    }
}