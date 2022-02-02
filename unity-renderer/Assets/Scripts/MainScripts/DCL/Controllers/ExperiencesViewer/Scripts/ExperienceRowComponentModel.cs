using System;
using UnityEngine;

[Serializable]
public class ExperienceRowComponentModel : BaseComponentModel
{
    public string id;
    public string iconUri;
    public string name;
    public bool isUIVisible;
    public bool isPlaying;
    public Color backgroundColor;
    public Color onHoverColor;
}