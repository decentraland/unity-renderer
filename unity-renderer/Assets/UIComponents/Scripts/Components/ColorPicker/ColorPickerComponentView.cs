using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;
using UnityEngine.UI;

public interface IColorPickerComponentView
{

    /// <summary>
    /// Event that will be triggered when the color changes
    /// </summary>
    public event System.Action<Color> OnColorChanged;

    /// <summary>
    /// Set preset color list
    /// </summary>
    void SetColorList(List<Color> colorList);

    /// <summary>
    /// Set the button increment amount
    /// </summary>
    void SetIncrementAmount(float amount);
}

public class ColorPickerComponentView : BaseComponentView, IColorPickerComponentView, IComponentModelConfig<ColorPickerComponentModel>
{

    [SerializeField] private SliderComponentView sliderHue;
    [SerializeField] private SliderComponentView sliderSaturation;
    [SerializeField] private SliderComponentView sliderValue;
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject container;
    [SerializeField] private Image containerImage;
    [SerializeField] private GameObject colorSelectorObject;
    [SerializeField] private Image colorPreviewImage;

    [Header("Configuration")]
    [SerializeField] internal ColorPickerComponentModel model;

    private IColorSelector colorSelector;
    public event Action<Color> OnColorChanged;
    private bool isAudioPlaying = false;

    override public void Awake()
    {
        base.Awake();

        colorSelector = colorSelectorObject.GetComponent<IColorSelector>();

        colorSelector.OnColorSelectorChange += UpdateValueFromColorSelector;

        sliderHue.onSliderChange.AddListener(hue => SetColor());
        sliderHue.onIncrement.AddListener(() => ChangeProperty("hue", model.incrementAmount));
        sliderHue.onDecrement.AddListener(() => ChangeProperty("hue", -model.incrementAmount));

        sliderSaturation.onSliderChange.AddListener(saturation => SetColor());
        sliderSaturation.onIncrement.AddListener(() => ChangeProperty("sat", model.incrementAmount));
        sliderSaturation.onDecrement.AddListener(() => ChangeProperty("sat", -model.incrementAmount));

        sliderValue.onSliderChange.AddListener(value => SetColor());
        sliderValue.onIncrement.AddListener(() => ChangeProperty("val", model.incrementAmount));
        sliderValue.onDecrement.AddListener(() => ChangeProperty("val", -model.incrementAmount));

        toggleButton.onClick.AddListener(() => SetActive(!container.activeInHierarchy));
        
        SetActive(false);
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetColorList(model.colorList);
        SetIncrementAmount(model.incrementAmount);
    }

    public void SetColorSelector(Color newColor) 
    {
        colorSelector.Select(newColor);
    }

    public void SetColorList(List<Color> colorList)
    {
        if(colorSelector == null)
            colorSelector = colorSelectorObject.GetComponent<IColorSelector>();

        model.colorList = colorList;
        colorSelector.Cleanup();
        colorSelector.Populate(colorList);
    }

    public void Configure(ColorPickerComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    private void ChangeProperty(string a, float amount)
    {
        switch (a)
        {
            case "hue":
                sliderHue.AddValueToSlider(amount);
                CheckButtonInteractability(sliderHue);
                break;
            case "sat":
                sliderSaturation.AddValueToSlider(amount);
                CheckButtonInteractability(sliderSaturation);
                break;
            case "val":
                sliderValue.AddValueToSlider(amount);
                CheckButtonInteractability(sliderValue);
                break;
            default:
                break;
        }
    }

    private void CheckButtonInteractability(SliderComponentView sliderComponent)
    {
        sliderComponent.incrementButton.interactable = (sliderComponent.slider.value < sliderComponent.slider.maxValue);
        sliderComponent.decrementButton.interactable = (sliderComponent.slider.value > sliderComponent.slider.minValue);
    }

    public void UpdateValueFromColorSelector(Color newColor)
    {
        UpdateSliderValues(newColor);
        SetColor();
    }

    public void UpdateSliderValues(Color currentColor)
    {
        Color.RGBToHSV(currentColor, out float h, out float s, out float v);
        colorPreviewImage.color = currentColor;
        sliderHue.slider.SetValueWithoutNotify(h);
        sliderSaturation.slider.SetValueWithoutNotify(s);
        sliderValue.slider.SetValueWithoutNotify(v);
    }

    private void SetColor()
    {
        Color newColor = Color.HSVToRGB(sliderHue.slider.value, sliderSaturation.slider.value, sliderValue.slider.value);
        colorPreviewImage.color = newColor;
        CheckButtonInteractability(sliderHue);
        CheckButtonInteractability(sliderSaturation);
        CheckButtonInteractability(sliderValue);
        if (!isAudioPlaying)
            StartCoroutine(PlaySound());

        OnColorChanged.Invoke(newColor);
    }

    private IEnumerator PlaySound() 
    {
        isAudioPlaying = true;
        AudioScriptableObjects.buttonRelease.Play(true);
        yield return new WaitForSeconds(0.05f);
        isAudioPlaying = false;
    }

    public void SetActive(bool isActive) 
    { 
        container.SetActive(isActive);
        containerImage.enabled = isActive;
    }

    override public void Dispose()
    {
        base.Dispose();
        toggleButton.onClick.RemoveAllListeners();
    }

    public void SetIncrementAmount(float amount) { model.incrementAmount = amount; }
}
