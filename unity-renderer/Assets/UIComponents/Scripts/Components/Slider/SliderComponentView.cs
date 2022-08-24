using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISliderComponentView
{
    /// <summary>
    /// Event that will be triggered when the slider value changes.
    /// </summary>
    Slider.SliderEvent onSliderChange { get; }

    /// <summary>
    /// Event that will be triggered when the increment button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onIncrement { get; }

    /// <summary>
    /// Event that will be triggered when the decrement button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onDecrement { get; }

    /// <summary>
    /// Set the slider text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetText(string newText);

    /// <summary>
    /// Set increment/decrement buttons active
    /// </summary>
    void SetButtonsActive(bool isActive);

    /// <summary>
    /// Set slider text active
    /// </summary>
    /// <param name="isActive">Is active</param>
    void SetTextActive(bool isActive);

    /// <summary>
    /// Set slider background
    /// </summary>
    /// <param name="image">Background image</param>
    void SetBackground(Sprite image);

    /// <summary>
    /// Set the slider interactable or not.
    /// </summary>
    /// <param name="isInteractable">Clickable or not</param>
    void SetInteractable(bool isInteractable);

    /// <summary>
    /// Returns true if the slider is Interactable
    /// </summary>
    bool IsInteractable();
}

public class SliderComponentView : BaseComponentView, ISliderComponentView, IComponentModelConfig<SliderComponentModel>
{

    [Header("Prefab References")]
    [SerializeField] public Slider slider;
    [SerializeField] internal Image background;
    [SerializeField] internal TMP_Text text;
    [SerializeField] public Button incrementButton;
    [SerializeField] public Button decrementButton;

    [Header("Configuration")]
    [SerializeField] internal SliderComponentModel model;

    public Slider.SliderEvent onSliderChange => slider?.onValueChanged;
    public Button.ButtonClickedEvent onIncrement => incrementButton?.onClick;
    public Button.ButtonClickedEvent onDecrement => decrementButton?.onClick;

    public void AddValueToSlider(float value)
    {
        slider.value += value;
    }

    public void Configure(SliderComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetButtonsActive(model.areButtonsActive);
        SetBackground(model.background);
        SetTextActive(model.isTextActive);
        SetText(model.text);
    }

    public bool IsInteractable() { return slider.interactable; }

    public void SetInteractable(bool isActive) { slider.interactable = isActive; }

    public void SetTextActive(bool isActive)
    {
        model.isTextActive = isActive;
        text.gameObject.SetActive(isActive);
    }

    public void SetButtonsActive(bool isActive)
    {
        model.areButtonsActive = isActive;
        incrementButton.gameObject.SetActive(isActive);
        decrementButton.gameObject.SetActive(isActive);
    }

    public void SetText(string newText)
    {
        model.text = newText;

        if (text == null)
            return;

        text.text = newText;
    }

    public void SetBackground(Sprite image)
    {
        model.background = image;

        if (image == null)
            return;

        background.sprite = image;
    }
}
