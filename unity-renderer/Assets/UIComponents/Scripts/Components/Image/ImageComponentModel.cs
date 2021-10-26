using System;
using UnityEngine;

[Serializable]
public class ImageComponentModel : BaseComponentModel
{
    public Sprite sprite;
    public Texture2D texture;
    public string uri;
    public bool fitParent = false;
}