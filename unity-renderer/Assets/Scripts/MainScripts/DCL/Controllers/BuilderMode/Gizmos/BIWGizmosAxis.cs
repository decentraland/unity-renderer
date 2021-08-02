using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEditor;
using UnityEngine;

public class BIWGizmosAxis : MonoBehaviour
{
    public Color defaultColor;
    public Color highLightColor;
    public Renderer objectRenderer;

    private MaterialPropertyBlock props;

    private BIWGizmos gizmo = null;

    public void SetGizmo(BIWGizmos parentGizmo) { gizmo = parentGizmo; }

    public BIWGizmos GetGizmo() { return gizmo; }

    public void SetColorHighlight()
    {
        if (props == null)
            return;

        objectRenderer.GetPropertyBlock(props);
        props.SetColor(ShaderUtils.BaseColor, highLightColor);
        objectRenderer.SetPropertyBlock(props);
    }

    public void SetColorDefault()
    {
        if (props == null)
            return;

        objectRenderer.GetPropertyBlock(props);
        props.SetColor(ShaderUtils.BaseColor, defaultColor);
        objectRenderer.SetPropertyBlock(props);
    }

    private void Start()
    {
        props = new MaterialPropertyBlock();
        objectRenderer.GetPropertyBlock(props);
        props.SetColor(ShaderUtils.BaseColor, defaultColor);
        objectRenderer.SetPropertyBlock(props);
    }
}