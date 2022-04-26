using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

[System.Serializable]
public class TextureModel 
{
    public enum BabylonWrapMode
    {
        CLAMP,
        WRAP,
        MIRROR
    }

    public string src;
    public BabylonWrapMode wrap = BabylonWrapMode.CLAMP;
    public FilterMode samplingMode = FilterMode.Bilinear;

    protected bool Equals(TextureModel other)
    {
        return src == other.src && wrap == other.wrap && samplingMode == other.samplingMode;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((TextureModel) obj);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (src != null ? src.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int) wrap;
            hashCode = (hashCode * 397) ^ (int) samplingMode;
            return hashCode;
        }
    }
}
