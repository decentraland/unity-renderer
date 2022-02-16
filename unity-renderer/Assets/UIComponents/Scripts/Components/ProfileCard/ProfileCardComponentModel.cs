using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ProfileCardComponentModel : BaseComponentModel
{
    public Sprite profilePictureSprite;
    public Texture2D profilePictureTexture;
    public string profilePictureUri;
    public string profileName;
    public string profileAddress;
    public bool isClaimedName;
}