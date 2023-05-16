using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ColorPickerComponentModel : BaseComponentModel
{
    public List<Color> colorList;
    public float incrementAmount;
    public bool showOnlyPresetColors;
}
