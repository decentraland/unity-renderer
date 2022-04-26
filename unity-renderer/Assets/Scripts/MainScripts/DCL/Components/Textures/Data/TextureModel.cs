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
}
