using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWGizmosAxis : MonoBehaviour
{
    public Color defaultColor;
    public Color highLightColor;
    public Renderer objectRenderer;

    MaterialPropertyBlock props;

    static int colorPropertyID;
    static bool isColorPropertyIdSet = false;

    private BIWGizmos gizmo = null;

    public void SetGizmo(BIWGizmos parentGizmo) { gizmo = parentGizmo; }

    public BIWGizmos GetGizmo() { return gizmo; }

    public void SetColorHighlight()
    {
        if (props == null)
            return;

        objectRenderer.GetPropertyBlock(props);
        props.SetColor(colorPropertyID, highLightColor);
        objectRenderer.SetPropertyBlock(props);

    }

    public void SetColorDefault()
    {
        if (props == null)
            return;

        objectRenderer.GetPropertyBlock(props);
        props.SetColor(colorPropertyID, defaultColor);
        objectRenderer.SetPropertyBlock(props);
    }

    private void Awake()
    {
        if (!isColorPropertyIdSet)
        {
            isColorPropertyIdSet = true;
            colorPropertyID = Shader.PropertyToID("_BaseColor");
        }
    }

    private void Start()
    {
        props = new MaterialPropertyBlock();
        objectRenderer.GetPropertyBlock(props);
        props.SetColor(colorPropertyID, defaultColor);
        objectRenderer.SetPropertyBlock(props);
    }
}