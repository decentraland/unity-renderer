using System;
using UnityEngine;

[Serializable]
public class ExperienceRowComponentModel : BaseComponentModel
{
    public Texture2D icon;
    public string name;
    public bool isUIVisible;
    public bool isPlaying;
    public Color backgroundColor;
    public Color onHoverColor;
}