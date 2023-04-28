using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPresets", menuName = "Variables/ColorPresets")]
public class ColorPresetsSO : ScriptableObject
{
    public List<Color> colors;
}
