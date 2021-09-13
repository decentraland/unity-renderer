using System;
using System.Collections.Generic;
using UnityEngine;

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
}