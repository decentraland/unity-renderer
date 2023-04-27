using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class ColorSelector : MonoBehaviour, IColorSelector
{
    [SerializeField] private ColorToggle colorTogglePrefab;
    [SerializeField] private RectTransform colorContainer;

    public event System.Action<Color> OnColorSelectorChange;

    private ColorToggle currentToggle;
    private readonly List<ColorToggle> colorToggles = new ();

    public void Populate(IReadOnlyList<Color> colors)
    {
        int colorNumber = colors.Count;

        for (var i = 0; i < colorNumber; ++i)
        {
            ColorToggle newToggle = Instantiate(colorTogglePrefab, colorContainer);
            newToggle.Initialize(colors[i], false);
            newToggle.OnClicked += ToggleClicked;
            colorToggles.Add(newToggle);
        }
    }

    public void Cleanup()
    {
        foreach (var colorToggle in colorToggles)
            DestroyImmediate(colorToggle);
    }

    public void Select(Color color)
    {
        ColorToggle toggle = GetColorToggle(color);

        if (toggle == currentToggle)
            return;

        Select(toggle);
    }

    public void SelectRandom()
    {
        if (colorToggles.Count == 0)
            return;

        ColorToggle toggle = colorToggles[Random.Range(0, colorToggles.Count)];
        if (toggle == currentToggle)
            return;

        Select(toggle);
        OnColorSelectorChange?.Invoke(currentToggle.Color);
    }

    private void Select(ColorToggle colorToggle)
    {
        if (currentToggle != null)
            currentToggle.Selected = false;

        currentToggle = colorToggle;

        if (colorToggle)
            colorToggle.Selected = true;
    }

    private void ToggleClicked(ColorToggle toggle)
    {
        if (toggle == currentToggle)
            return;

        Select(toggle);
        OnColorSelectorChange?.Invoke(currentToggle.Color);
    }

    private ColorToggle GetColorToggle(Color color)
    {
        int colorTogglesCount = colorToggles.Count;
        for (var i = 0; i < colorTogglesCount; i++)
        {
            Color current = colorToggles[i].Color;
            if (color.AproxComparison(current))
                return colorToggles[i];
        }

        return null;
    }
}
