using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorPickerView : MonoBehaviour
{

    [Header("Prefab References")]
    [SerializeField] internal Button toggleButton;
    [SerializeField] internal GameObject container;
    [SerializeField] internal Slider hueSlider;
    [SerializeField] internal Slider saturationSlider;
    [SerializeField] internal Slider valueSlider;

    public event System.Action<Color> OnColorChanged;

    void Start()
    {
        hueSlider.onValueChanged.AddListener(hue => SetColor());
        saturationSlider.onValueChanged.AddListener(saturation => SetColor());
        valueSlider.onValueChanged.AddListener(value => SetColor());
        toggleButton.onClick.AddListener(() => SetActive(!container.activeInHierarchy));
        SetActive(false);
    }

    public void UpdateSliderValues(Color currentColor) 
    {
        Color.RGBToHSV(currentColor, out float hue, out float saturation, out float value);
        hueSlider.SetValueWithoutNotify(hue);
        saturationSlider.SetValueWithoutNotify(saturation);
        valueSlider.SetValueWithoutNotify(value);
    }

    public void SetRandomColor()
    {
        OnColorChanged.Invoke(Color.HSVToRGB(Random.Range(0,1), Random.Range(0,1), Random.Range(0,1)));
    }

    private void SetColor() 
    {
        OnColorChanged.Invoke(Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value));
    }

    private void SetActive(bool isActive) 
    {
        container.SetActive(isActive);
    }

}
