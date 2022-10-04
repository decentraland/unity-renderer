using UnityEngine;

public static class ObjectsOutlinerUtils
{
    private static readonly int _OverlayColor = Shader.PropertyToID("_OverlayColor");
    private static readonly int _Overlay = Shader.PropertyToID("_Overlay");
    private static readonly int _OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int _OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int _OutlineAmount = Shader.PropertyToID("_OutlineAmount");
    private static readonly int _OutlineBasedVertexColorR = Shader.PropertyToID("_OutlineBasedVertexColorR");
    private static readonly int _ZTest2ndPass = Shader.PropertyToID("_ZTest2ndPass");
    private static readonly int _ZTest3rdPass = Shader.PropertyToID("_ZTest3rdPass");

    public static void PrepareMaterial(Material material, OutlinerConfig config)
    {
        if (config.outlineRenderMethod == OutlineRenderMethod.OutlineAndFill)
        {
            // overlay color
            material.SetColor(_OverlayColor, config.overlayColor);
            // overlay amount
            material.SetFloat(_Overlay, config.overlayAmount);
        }

        if (config.outlineRenderMethod != OutlineRenderMethod.OutlineAndFill)
        {
            // overlay amount
            material.SetFloat(_Overlay, config.overlayAmount);
        }

        // overlay color
        material.SetColor(_OverlayColor, config.overlayColor);

        material.SetFloat(_OutlineWidth, config.outlineWidth);
        material.SetColor(_OutlineColor, config.outlineColor);

        material.SetFloat(_OutlineAmount, config.outlineAmount);
        material.SetFloat(_OutlineBasedVertexColorR, config.vertexColorRedChannel ? 0f : 1f);


        // control passes
        switch (config.outlineRenderMethod)
        {
            case OutlineRenderMethod.OutlineStandard:

                material.SetInt(_ZTest2ndPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                material.SetInt(_ZTest3rdPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                break;

            case OutlineRenderMethod.OutlineVisible:

                material.SetInt(_ZTest2ndPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                material.SetInt(_ZTest3rdPass, (int)UnityEngine.Rendering.CompareFunction.LessEqual);

                break;
            case OutlineRenderMethod.OutlineOccluded:

                material.SetInt(_ZTest2ndPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                material.SetInt(_ZTest3rdPass, (int)UnityEngine.Rendering.CompareFunction.Greater);

                break;
            case OutlineRenderMethod.OutlineAndFill:

                material.SetInt(_ZTest2ndPass, (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                material.SetInt(_ZTest3rdPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                break;

            default:
                material.SetInt(_ZTest2ndPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                material.SetInt(_ZTest3rdPass, (int)UnityEngine.Rendering.CompareFunction.Always);
                break;
        }
    }
}