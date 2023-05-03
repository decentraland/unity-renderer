using System.Collections.Generic;
using UnityEngine;

public interface IColorSelector
{
    public event System.Action<Color> OnColorSelectorChange;
    public void Populate(IReadOnlyList<Color> colors);
    public void Cleanup();
    public void Select(Color color);
    void SelectRandom();
}
