# Slider

Implements: `BaseComponentView, ISliderComponentView, IComponentModelConfig`

Model: `SliderComponentModel`

### Preview

![Screenshot 2022-05-05 at 09.41.27.png](slider/Screenshot_2022-05-05_at_09.41.27.png)

### Description

A generic slider component that allows customization

### Configuration (Model)

- `text`: Text that will be showed in the slider.
- `isTextActive`: Boolean that sets the text active or not.
- `areButtonsActive`: Boolean that sets the increment and decrement buttons active or not.
- `background`: Sprite that substitutes the background image of the slider.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the slider component with this data.
- `SliderEvent onSliderChange`: Event that will be triggered when the slider value changes.
- `ButtonClickedEvent onIncrement`: Event that will be triggered when the increment button is pressed.
- `ButtonClickedEvent onDecrement`: Event that will be triggered when the decrement button is pressed.
- `void SetText(string newText)`: Set the slider text.
- `void SetTextActive(bool isActive)`: Set slider text active.
- `void SetBackground(Sprite image)`: Set slider background sprite.
- `void SetInteractable(bool isInteractable)`: Set the slider interactable or not.
- `bool IsInteractable()`: Return if the slider is interactable or not.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.