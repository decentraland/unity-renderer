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
    
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = alphaTest.GetHashCode();
            hashCode = (hashCode * 397) ^ albedoColor.GetHashCode();
            hashCode = (hashCode * 397) ^ metallic.GetHashCode();
            hashCode = (hashCode * 397) ^ roughness.GetHashCode();
            hashCode = (hashCode * 397) ^ microSurface.GetHashCode();
            hashCode = (hashCode * 397) ^ specularIntensity.GetHashCode();
            hashCode = (hashCode * 397) ^ emissiveColor.GetHashCode();
            hashCode = (hashCode * 397) ^ emissiveIntensity.GetHashCode();
            hashCode = (hashCode * 397) ^ reflectivityColor.GetHashCode();
            hashCode = (hashCode * 397) ^ directIntensity.GetHashCode();
            hashCode = (hashCode * 397) ^ castShadows.GetHashCode();
            hashCode = (hashCode * 397) ^ transparencyMode;
            hashCode = (hashCode * 397) ^ (albedoTexture != null ? albedoTexture.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (alphaTexture != null ? alphaTexture.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (emissiveTexture != null ? emissiveTexture.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (bumpTexture != null ? bumpTexture.GetHashCode() : 0);
            return hashCode;
        }
    }
    
    protected bool Equals(MaterialModel other)
    {
        return alphaTest.Equals(other.alphaTest) && albedoColor.Equals(other.albedoColor) && metallic.Equals(other.metallic) && roughness.Equals(other.roughness) && microSurface.Equals(other.microSurface) && specularIntensity.Equals(other.specularIntensity) && emissiveColor.Equals(other.emissiveColor) && emissiveIntensity.Equals(other.emissiveIntensity) && reflectivityColor.Equals(other.reflectivityColor) && directIntensity.Equals(other.directIntensity) && castShadows == other.castShadows && transparencyMode == other.transparencyMode && Equals(albedoTexture, other.albedoTexture) && Equals(alphaTexture, other.alphaTexture) && Equals(emissiveTexture, other.emissiveTexture) && Equals(bumpTexture, other.bumpTexture);
    }
    
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((MaterialModel) obj);
    }
}
