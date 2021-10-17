using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SectionSelectorComponentModel : BaseComponentModel
{
    public List<SectionToggleModel> sections;
}

[Serializable]
public class SectionToggleModel
{
    public Sprite icon;
    public string title;
    public Toggle.ToggleEvent onSelect;
    public Color selectedBackgroundColor;
    public Color selectedTextColor;
    public Color selectedImageColor;
    public Color unselectedBackgroundColor;
    public Color unselectedTextColor;
    public Color unselectedImageColor;
}