using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ColorPickerComponentModel : BaseComponentModel
{
    public List<Color> colorList;
    public float incrementAmount;
}
