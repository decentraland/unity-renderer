using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class ColorPickerView : MonoBehaviour
{

    private const float BUTTON_INCREMENT = 0.1f;

    [Serializable]
    public class PropertySlider
    {
        [SerializeField] internal Slider propSlider;
        [SerializeField] internal Button increaseButton;
        [SerializeField] internal Button decreaseButton;
    }

    [Header("Prefab References")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image colorPreviewImage;
    [SerializeField] private GameObject container;
    [SerializeField] private PropertySlider hue;
    [SerializeField] private PropertySlider saturation;
    [SerializeField] private PropertySlider value;

    public event System.Action<Color> OnColorChanged;

    void Start()
    {
        hue.propSlider.onValueChanged.AddListener(hue => SetColor());
        hue.increaseButton.onClick.AddListener(() => ChangeProperty("hue", BUTTON_INCREMENT));
        hue.decreaseButton.onClick.AddListener(() => ChangeProperty("hue", -BUTTON_INCREMENT));

        saturation.propSlider.onValueChanged.AddListener(saturation => SetColor());
        saturation.increaseButton.onClick.AddListener(() => ChangeProperty("sat", BUTTON_INCREMENT));
        saturation.decreaseButton.onClick.AddListener(() => ChangeProperty("sat", -BUTTON_INCREMENT));

        value.propSlider.onValueChanged.AddListener(value => SetColor());
        value.increaseButton.onClick.AddListener(() => ChangeProperty("val", BUTTON_INCREMENT));
        value.decreaseButton.onClick.AddListener(() => ChangeProperty("val", -BUTTON_INCREMENT));

        toggleButton.onClick.AddListener(() => SetActive(!container.activeInHierarchy));
        SetActive(false);
    }

    private void ChangeProperty(string a, float amount)
    {
        switch (a) {
            case "hue":
                hue.propSlider.value += amount;
                CheckButtonInteractability(hue);
                break;
            case "sat":
                saturation.propSlider.value += amount;
                CheckButtonInteractability(saturation);
                break;
            case "val":
                value.propSlider.value += amount;
                CheckButtonInteractability(value);
                break;
            default:
                break;
        }
    }

    private void CheckButtonInteractability(PropertySlider slider) 
    {
        slider.increaseButton.interactable = (slider.propSlider.value < slider.propSlider.maxValue);
        slider.decreaseButton.interactable = (slider.propSlider.value > slider.propSlider.minValue);
    }

    public void UpdateSliderValues(Color currentColor) 
    {
        Color.RGBToHSV(currentColor, out float h, out float s, out float v);
        colorPreviewImage.color = currentColor;
        hue.propSlider.SetValueWithoutNotify(h);
        saturation.propSlider.SetValueWithoutNotify(s);
        value.propSlider.SetValueWithoutNotify(v);
    }

    public void SetRandomColor()
    {
        Color newColor = Color.HSVToRGB(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
        colorPreviewImage.color = newColor;
        OnColorChanged.Invoke(newColor);
    }

    private void SetColor() 
    {
        Color newColor = Color.HSVToRGB(hue.propSlider.value, saturation.propSlider.value, value.propSlider.value);
        colorPreviewImage.color = newColor;
        CheckButtonInteractability(hue);
        CheckButtonInteractability(saturation);
        CheckButtonInteractability(value);
        OnColorChanged.Invoke(newColor);
    }

    private void SetActive(bool isActive) 
    {
        container.SetActive(isActive);
    }

}
