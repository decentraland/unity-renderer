using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerComponentView : BaseComponentView, IComponentModelConfig<ColorPickerComponentModel>
{
    [SerializeField] private SliderComponentView sliderHue;
    [SerializeField] private SliderComponentView sliderSaturation;
    [SerializeField] private SliderComponentView sliderValue;
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject container;
    [SerializeField] private Image containerImage;
    [SerializeField] private GameObject colorSelectorObject;
    [SerializeField] private Image colorPreviewImage;
    [SerializeField] private GameObject arrowUpMark;
    [SerializeField] private GameObject arrowDownMark;

    [Header("Configuration")]
    [SerializeField] internal ColorPickerComponentModel model;

    public bool IsShowingOnlyPresetColors => model.showOnlyPresetColors;
    public List<Color> ColorList => model.colorList;
    public Color CurrentSelectedColor { get; private set; }

    public event Action<Color> OnColorChanged;

    private IColorSelector colorSelector;
    private bool isAudioPlaying;

    public override void Awake()
    {
        base.Awake();

        colorSelector = colorSelectorObject.GetComponent<IColorSelector>();
        colorSelector.OnColorSelectorChange += UpdateValueFromColorSelector;

        sliderHue.onSliderChange.AddListener(_ => SetColor());
        sliderHue.onIncrement.AddListener(() => ChangeProperty("hue", model.incrementAmount));
        sliderHue.onDecrement.AddListener(() => ChangeProperty("hue", -model.incrementAmount));

        sliderSaturation.onSliderChange.AddListener(_ => SetColor());
        sliderSaturation.onIncrement.AddListener(() => ChangeProperty("sat", model.incrementAmount));
        sliderSaturation.onDecrement.AddListener(() => ChangeProperty("sat", -model.incrementAmount));

        sliderValue.onSliderChange.AddListener(_ => SetColor());
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
        SetShowOnlyPresetColors(model.showOnlyPresetColors);
    }

    public void SetColorSelector(Color newColor)
    {
        colorSelector.Select(newColor);
        CurrentSelectedColor = newColor;
    }

    public void SetColorList(List<Color> colorList)
    {
        model.colorList = colorList;
        colorSelector ??= colorSelectorObject.GetComponent<IColorSelector>();
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
                CheckButtonInteractivity(sliderHue);
                break;
            case "sat":
                sliderSaturation.AddValueToSlider(amount);
                CheckButtonInteractivity(sliderSaturation);
                break;
            case "val":
                sliderValue.AddValueToSlider(amount);
                CheckButtonInteractivity(sliderValue);
                break;
        }
    }

    private static void CheckButtonInteractivity(SliderComponentView sliderComponent)
    {
        sliderComponent.incrementButton.interactable = sliderComponent.slider.value < sliderComponent.slider.maxValue;
        sliderComponent.decrementButton.interactable = sliderComponent.slider.value > sliderComponent.slider.minValue;
    }

    private void UpdateValueFromColorSelector(Color newColor)
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
        CheckButtonInteractivity(sliderHue);
        CheckButtonInteractivity(sliderSaturation);
        CheckButtonInteractivity(sliderValue);
        if (!isAudioPlaying)
            StartCoroutine(PlaySound());

        OnColorChanged?.Invoke(newColor);
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

        if (arrowUpMark != null)
            arrowUpMark.SetActive(isActive);

        if (arrowDownMark != null)
            arrowDownMark.SetActive(!isActive);

        if (isActive)
            RebuildLayout();
    }

    public override void Dispose()
    {
        base.Dispose();
        toggleButton.onClick.RemoveAllListeners();
    }

    public void SetIncrementAmount(float amount) =>
        model.incrementAmount = amount;

    public void SetShowOnlyPresetColors(bool showOnlyPresetColors)
    {
        model.showOnlyPresetColors = showOnlyPresetColors;

        sliderHue.gameObject.SetActive(!showOnlyPresetColors);
        sliderSaturation.gameObject.SetActive(!showOnlyPresetColors);
        sliderValue.gameObject.SetActive(!showOnlyPresetColors);

        RebuildLayout();
    }

    private void RebuildLayout() =>
        Utils.ForceRebuildLayoutImmediate(transform as RectTransform);
}
