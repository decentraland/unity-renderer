using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDCLTextureResource 
{
    Texture2D texture2D { get; }
    // void getTexture2D(Action<Texture2D> onSuccess, Action<string> onFail);
}
