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
    [Header("Common Configuration")]
    public bool showNewTag = false;

    [Header("Visual Configuration When Selected")]
    public Sprite selectedIcon;
    public string selectedTitle;
    public Color selectedTextColor;
    public Color selectedImageColor;
    public ColorBlock backgroundTransitionColorsForSelected;

    [Header("Visual Configuration When Unselected")]
    public Sprite unselectedIcon;
    public string unselectedTitle;
    public Color unselectedTextColor;
    public Color unselectedImageColor;
    public ColorBlock backgroundTransitionColorsForUnselected;
}
