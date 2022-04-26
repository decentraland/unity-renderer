using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

[System.Serializable]
public class MaterialModel
{
    public enum TransparencyMode
    {
        OPAQUE,
        ALPHA_TEST,
        ALPHA_BLEND,
        ALPHA_TEST_AND_BLEND,
        AUTO
    }
    
    [Range(0f, 1f)]
    public float alphaTest = 0.5f;

    public Color albedoColor = Color.white;

    public float metallic = 0.5f;
    public float roughness = 0.5f;
    public float microSurface = 1f; // Glossiness
    public float specularIntensity = 1f;
    
    public Color emissiveColor = Color.black;
    public float emissiveIntensity = 2f;
    public Color reflectivityColor = Color.white;
    public float directIntensity = 1f;

    public bool castShadows = true;
    
    [Range(0, 4)]
    public int transparencyMode = 4; // 0: OPAQUE; 1: ALPHATEST; 2: ALPHBLEND; 3: ALPHATESTANDBLEND; 4: AUTO (Engine decide)
    
    public TextureModel albedoTexture;
    public TextureModel alphaTexture;
    public TextureModel emissiveTexture;
    public TextureModel bumpTexture;
}
