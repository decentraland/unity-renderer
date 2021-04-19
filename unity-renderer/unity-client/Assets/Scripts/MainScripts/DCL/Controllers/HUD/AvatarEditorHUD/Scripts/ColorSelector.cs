using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class ColorSelector : MonoBehaviour
{
    [SerializeField]
    private ColorToggle colorTogglePrefab;

    [SerializeField]
    private RectTransform colorContainer;

    public event System.Action<Color> OnColorChanged;

    private ColorToggle currentToggle;

    private List<ColorToggle> colorToggles = new List<ColorToggle>();

    public void Populate(IReadOnlyList<Color> colors)
    {
        int colorNumber = colors.Count;

        for (int i = 0; i < colorNumber; ++i)
        {
            ColorToggle newToggle = Instantiate(colorTogglePrefab, colorContainer);
            newToggle.Initialize(colors[i], false);
            newToggle.OnClicked += ToggleClicked;
            colorToggles.Add(newToggle);
        }
    }

    public void Select(Color color)
    {
        ColorToggle toggle = GetColorToggle(color);
        if (toggle == currentToggle) return;
        Select(toggle);
    }

    public void SelectRandom()
    {
        if (colorToggles.Count == 0) return;

        ColorToggle toggle = colorToggles[Random.Range(0, colorToggles.Count)];
        if (toggle == currentToggle) return;

        Select(toggle);
        OnColorChanged?.Invoke(currentToggle.color);
    }

    private void Select(ColorToggle colorToggle)
    {
        if (currentToggle != null)
        {
            currentToggle.selected = false;
        }

        currentToggle = colorToggle;

        if (colorToggle)
        {
            colorToggle.selected = true;
        }
    }

    private void ToggleClicked(ColorToggle toggle)
    {
        if (toggle == currentToggle) return;

        Select(toggle);
        OnColorChanged?.Invoke(currentToggle.color);
    }

    private ColorToggle GetColorToggle(Color color)
    {
        int colorTogglesCount = colorToggles.Count;
        for (int i = 0; i < colorTogglesCount; i++)
        {
            Color current = colorToggles[i].color;
            if (color.AproxComparison(current))
            {
                return colorToggles[i];
            }
        }

        return null;
    }
}