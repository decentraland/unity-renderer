using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SectionSelectorComponentModel
{
    public List<SectionToggleModel> sections;
}

[Serializable]
public class SectionToggleModel
{
    public Sprite icon;
    public string title;
    public Toggle.ToggleEvent onSelectEvent;
}