using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Helpers;
using UnityEditor;
using UnityEngine;

public class BIWGizmosAxis : MonoBehaviour, IBIWGizmosAxis
{
    public Color defaultColor;
    public Color highLightColor;
    public Renderer objectRenderer;

    private MaterialPropertyBlock props;

    private IBIWGizmos gizmo = null;

    public void SetGizmo(IBIWGizmos parentGizmo) { gizmo = parentGizmo; }

    public IBIWGizmos GetGizmo() { return gizmo; }
    public Transform axisTransform  => transform;

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