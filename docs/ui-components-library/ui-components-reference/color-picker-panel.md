# ColorPickerPanel

Implements: `BaseComponentView, IColorPickerComponentView, IComponentModelConfig`

Model: `ColorPickerModel`

### Preview

![Screenshot 2022-05-05 at 09.45.19.png](color-picker-panel/Screenshot_2022-05-05_at_09.45.19.png)

### Description

A panel that provides sliders and color presets to change any color, providing an event passing the new color.

### Configuration (Model)

- `List<Color>`: List containing the colors that will be added to the preset list.
- `incrementAmount`: float indicating how much the sliders will increment or decrement when the right and left arrows are pressed.

### Exposed Properties/Methods

- `void Configure(BaseComponentModel newModel)`: Fills the model and refreshes the color picker component with this data.
- `Action<Color> OnColorChanged`: Event that will be triggered when the color changes.
- `void SetColorList(List<Color> colorList)`: Set the color presets.
- `void SetIncrementAmount(float amount)`: Set the slider increment/decrement amount per click.

### How To Use

1. Drag the prefab to your scene.
2. Configure the model from the inspector.
3. Click on **[REFRESH]**.
4. From the scripts that wants to get the new color, subscribe to the `OnColorChanged` event and read the passed `Color`