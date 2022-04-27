using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NFTShapeBuilderRepresentantion
{
    public string url;
}

[Serializable]
public class NFTShapeStatelessRepresentantion
{
    public string src;
    public string assetId;
    public ColorRepresentantion color = new ColorRepresentantion(0.6404918f, 0.611472f, 0.8584906f); // "light purple" default, same as in explorer
    public int style = 0;
    public bool withCollisions = true;
    public bool isPointerBlocker = true;
    public bool visible = true;


    [Serializable]
    public class ColorRepresentantion
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public ColorRepresentantion(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;

            a = 1;
        }
    }
}