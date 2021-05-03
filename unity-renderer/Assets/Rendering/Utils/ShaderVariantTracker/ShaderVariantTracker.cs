using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;

public class ShaderVariantTracker : IDisposable
{
    private ShaderVariantCollection collection;

    public ShaderVariantTracker (ShaderVariantCollection collection)
    {
        this.collection = collection;

        SRPBatchingHelper.OnMaterialProcess += OnMaterialProcess;
    }

    private void OnMaterialProcess(Material mat)
    {
        var variant = CreateVariant(mat);

        if ( !collection.Contains(variant) )
        {
            collection.Add(variant);
            var keywordsString = JsonUtility.ToJson(variant.keywords, true);
            Debug.Log($"New variant found!\nShader: {variant.shader.name}\nKeywords:{keywordsString}");
        }
    }

    static ShaderVariantCollection.ShaderVariant CreateVariant(Material mat)
    {
        var result = new ShaderVariantCollection.ShaderVariant();
        result.shader = mat.shader;
        result.keywords = mat.shaderKeywords;
        result.passType = PassType.ScriptableRenderPipeline;
        return result;
    }

    public void Dispose() { SRPBatchingHelper.OnMaterialProcess -= OnMaterialProcess; }
}