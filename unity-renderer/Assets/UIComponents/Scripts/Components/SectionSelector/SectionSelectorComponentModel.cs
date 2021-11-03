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

    [Header("Visual Configuration When Selected")]
    public Color selectedTextColor;
    public Color selectedImageColor;
    public ColorBlock backgroundTransitionColorsForSelected;

    [Header("Visual Configuration When Unselected")]
    public Color unselectedTextColor;
    public Color unselectedImageColor;
    public ColorBlock backgroundTransitionColorsForUnselected;
}