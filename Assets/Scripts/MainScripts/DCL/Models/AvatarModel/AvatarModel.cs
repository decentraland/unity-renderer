using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarModel
{
    public string name;
    public string bodyShape;
    public Color skinColor;
    public Color hairColor;
    public Color eyeColor;
    public List<string> wearables = new List<string>();

    public void CopyFrom (AvatarModel other)
    {
        if (other == null) return;

        name = other.name;
        bodyShape = other.bodyShape;
        skinColor = other.skinColor;
        hairColor = other.hairColor;
        eyeColor = other.eyeColor;
        wearables = new List<string>(other.wearables);
    }
}